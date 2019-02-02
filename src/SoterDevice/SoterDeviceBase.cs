/*
 * Copyright (C) 2018 Touchjet Limited.
 * 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtoBuf;
using Serilog;
using SoterDevice.Contracts;
using SoterDevice.Models;
using SoterDevice.Utilities;
using Touchjet.BinaryUtils;

namespace SoterDevice
{
    public abstract class SoterDeviceBase : ISoterDevice
    {
        public ButtonRequestHandler DeviceButtonRequestCallback { get; set; }

        public EnterPinHandler EnterPinCallback { get; set; }

        SemaphoreSlim _Lock = new SemaphoreSlim(1, 1);
        static readonly Dictionary<string, Type> _ContractsByName = new Dictionary<string, Type>();

        protected object _LastWrittenMessage;

        protected abstract Task WriteAsync(object msg);

        protected abstract Task<object> ReadAsync();

        protected Type MessageTypeType => typeof(MessageType);

        public abstract void Disconnect();

        protected int _invalidRxChunksCounter;

        public bool IsInitialized => Features != null;

        public Features Features { get; private set; }

        public string Name { get; protected set; }

        public string Id { get; protected set; }

        public ICoinUtility CoinUtility { get; set; }

        public virtual int Mtu => throw new NotImplementedException();

        public Task<string> GetAddressAsync(IAddressPath addressPath, bool isPublicKey, bool display)
        {
            if (CoinUtility == null)
            {
                throw new DeviceException($"A {nameof(CoinUtility)} must be specified if {nameof(AddressType)} is not specified.");
            }

            var cointType = addressPath.AddressPathElements.Count > 1 ? addressPath.AddressPathElements[1].Value : throw new DeviceException("The first element of the address path is considered to be the coin type. This was not specified so no coin information is available. Please use an overload that specifies CoinInfo.");

            var coinInfo = CoinUtility.GetCoinInfo(cointType);

            return GetAddressAsync(addressPath, isPublicKey, display, coinInfo);
        }

        public Task<string> GetAddressAsync(IAddressPath addressPath, bool isPublicKey, bool display, CoinInfo coinInfo)
        {
            var inputScriptType = coinInfo.IsSegwit ? InputScriptType.Spendp2shwitness : InputScriptType.Spendaddress;

            return GetAddressAsync(addressPath, isPublicKey, display, coinInfo.AddressType, inputScriptType, coinInfo.CoinName);
        }

        public Task<string> GetAddressAsync(IAddressPath addressPath, bool isPublicKey, bool display, AddressType addressType, InputScriptType inputScriptType)
        {
            return GetAddressAsync(addressPath, isPublicKey, display, addressType, inputScriptType, null);
        }

        public async Task<string> GetAddressAsync(IAddressPath addressPath, bool isPublicKey, bool display, AddressType addressType, InputScriptType inputScriptType, string coinName)
        {
            try
            {
                var path = addressPath.ToArray();

                if (isPublicKey)
                {
                    var publicKey = await SendMessageAsync<PublicKey, GetPublicKey>(new GetPublicKey { AddressNs = path, ShowDisplay = display, ScriptType = inputScriptType });
                    return publicKey.Xpub;
                }
                else
                {
                    switch (addressType)
                    {
                        case AddressType.Bitcoin:
                            return (await SendMessageAsync<Address, GetAddress>(new GetAddress { ShowDisplay = display, AddressNs = path, CoinName = coinName, ScriptType = inputScriptType })).address;
                        case AddressType.Ethereum:
                            var ethereumAddress = await SendMessageAsync<EthereumAddress, EthereumGetAddress>(new EthereumGetAddress { ShowDisplay = display, AddressNs = path });
                            return $"0x{ethereumAddress.Address.ToHex().ToLower()}";
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error Getting KeepKey Address \n {ex}");
                throw;
            }
        }

        void LogMessage(object message, bool tx)
        {
            var jsonSerializerSetting = new JsonSerializerSettings { Converters = { new JsonByteArrayHexConverter() } };
            StringBuilder logStringBuilder = new StringBuilder();
            logStringBuilder.Append("Message ");
            logStringBuilder.Append(tx ? "--> " : "<-- ");
            logStringBuilder.Append(message.GetType().ToString().Substring(22));
            logStringBuilder.Append(JsonConvert.SerializeObject(message, jsonSerializerSetting));
            Log.Debug(logStringBuilder.ToString());
        }

        public async Task<TReadMessage> SendMessageAsync<TReadMessage, TWriteMessage>(TWriteMessage message)
        {
            ValidateInitialization(message);

            await _Lock.WaitAsync();

            try
            {
                var response = await SendMessageAsync(message);

                for (var i = 0; i < 10; i++)
                {
                    if (IsPinMatrixRequest(response))
                    {
                        var pin = await EnterPinCallback.Invoke((response as PinMatrixRequest).Type);
                        response = await PinMatrixAckAsync(pin);
                        if (response is TReadMessage readMessage)
                        {
                            return readMessage;
                        }
                    }

                    else if (IsButtonRequest(response))
                    {
                        DeviceButtonRequestCallback?.Invoke();
                        response = await ButtonAckAsync();

                        if (response is TReadMessage readMessage)
                        {
                            return readMessage;
                        }
                    }

                    else if (response is TReadMessage readMessage)
                    {
                        return readMessage;
                    }
                }

                throw new DeviceException($"Returned response ({response.GetType().Name})  was of the wrong specified message type ({typeof(TReadMessage).Name}). The user did not accept the message, or pin was entered incorrectly too many times (Note: this library does not have an incorrect pin safety mechanism.)");
            }
            finally
            {
                _Lock.Release();
            }
        }

        protected async Task<object> SendMessageAsync(object message)
        {
            LogMessage(message, true);
            await WriteAsync(message);
            var retVal = await ReadAsync();
            LogMessage(retVal, false);

            CheckForFailure(retVal);

            return retVal;
        }

        bool IsButtonRequest(object response)
        {
            return response is ButtonRequest;
        }

        async Task<object> ButtonAckAsync()
        {
            var retVal = await SendMessageAsync(new ButtonAck());

            if (retVal is Failure failure)
            {
                throw new FailureException<Failure>("ButtonAck Failure.", failure);
            }

            return retVal;
        }

        bool IsPinMatrixRequest(object response)
        {
            return response is PinMatrixRequest;
        }

        async Task<object> PinMatrixAckAsync(string pin)
        {
            object retVal = await SendMessageAsync(new PinMatrixAck { Pin = pin });

            if (retVal is Failure failure)
            {
                throw new FailureException<Failure>("PIN Attempt Failed.", failure);
            }

            return retVal;
        }

        bool IsInitialize(object response)
        {
            return response is Initialize;
        }

        void ValidateInitialization(object message)
        {
            if (!IsInitialized && !IsInitialize(message))
            {
                throw new DeviceException($"The device has not been successfully initialised. Please call {nameof(InitializeAsync)}.");
            }
        }

        protected static byte[] Serialize(object msg)
        {
            using (var writer = new MemoryStream())
            {
                Serializer.NonGeneric.Serialize(writer, msg);
                return writer.ToArray();
            }
        }

        protected object Deserialize(MessageType messageType, byte[] data)
        {
            try
            {
                var contractType = ContractUtility.GetContractType(messageType);

                return Deserialize(contractType, data);
            }
            catch
            {
                throw new Exception("InvalidProtocolBufferException");
            }
        }

        protected static object Deserialize(Type type, byte[] data)
        {
            using (var writer = new MemoryStream(data))
            {
                return Serializer.NonGeneric.Deserialize(type, writer);
            }
        }

        protected void CheckForFailure(object returnMessage)
        {
            if (returnMessage is Failure failure)
            {
                throw new FailureException<Failure>($"Error sending message to the device.\r\nCode: {failure.Code} Message: {failure.Message}", failure);
            }
        }

        protected object GetEnumValue(string messageTypeString)
        {
            var isValid = Enum.TryParse(messageTypeString, out MessageType messageType);
            if (!isValid)
            {
                throw new Exception($"{messageTypeString} is not a valid MessageType");
            }

            return messageType;
        }

        public async Task InitializeAsync()
        {
            Features = await SendMessageAsync<Features, Initialize>(new Initialize());

            if (Features == null)
            {
                throw new DeviceException("Error initializing Soter Wallet. Features were not retrieved");
            }
        }

        public async Task<IEnumerable<CoinType>> GetCoinTableAsync(uint maxRecords = 0)
        {
            var coinInfos = new List<CoinType>();
            var coinTable = await SendMessageAsync<CoinTable, GetCoinTable>(new GetCoinTable());

            uint coinIndex = 0;
            uint numOfCoins = maxRecords == 0 ? coinTable.NumCoins : Math.Min(maxRecords, coinTable.NumCoins);
            uint chunkSize = coinTable.ChunkSize;
            while (coinIndex < numOfCoins)
            {
                coinTable = await SendMessageAsync<CoinTable, GetCoinTable>(new GetCoinTable { Start = coinIndex, End = Math.Min(coinIndex + chunkSize, coinTable.NumCoins - 1) });
                foreach (var coinType in coinTable.Tables)
                {
                    coinInfos.Add(coinType);
                }
                coinIndex += chunkSize;
            }
            return coinInfos;
        }

        public uint MnemonicWordCountToKeyStrength(uint wordCount)
        {
            switch (wordCount)
            {
                case 12:
                    return 128;
                case 18:
                    return 192;
                case 24:
                    return 256;
                default:
                    throw new ArgumentException("Invalid Mnemonic Word Count!");
            }
        }

        public async Task ResetDeviceAsync(string deviceName, uint mnemonicWordCount = 12, string language = "english")
        {
            Random rnd = new Random();
            var entropyRequest = await SendMessageAsync<EntropyRequest, ResetDevice>(new ResetDevice { DisplayRandom = false, Strength = MnemonicWordCountToKeyStrength(mnemonicWordCount), PassphraseProtection = false, PinProtection = true, Language = language, Label = deviceName });
            byte[] randBytes = new byte[32];
            rnd.NextBytes(randBytes);
            var entropy = new EntropyAck() { Entropy = randBytes };
            var success = await SendMessageAsync<Success, EntropyAck>(entropy);
        }

        public async Task WipeDeviceAsync()
        {
            var success = await SendMessageAsync<Success, WipeDevice>(new WipeDevice());
            Log.Debug(success.Message);
        }

        public async Task CancelAsync()
        {
            try
            {
                await SendMessageAsync<Failure, Cancel>(new Cancel());
            }
            catch (FailureException<Failure> failure)
            {
                Log.Debug(failure.Message);
            }
        }

        public async Task ChangePinAsync(bool remove = false)
        {
            var success = await SendMessageAsync<Success, ChangePin>(new ChangePin() { Remove = remove });
            Log.Debug(success.Message);
        }

        public async Task ChangeAutoLockDelayAsync(uint ms)
        {
            var success = await SendMessageAsync<Success, ApplySettings>(new ApplySettings() { AutoLockDelayMs = ms });
            Log.Debug(success.Message);
        }

        public async Task ChangeDeviceNameAsync(string deviceName)
        {
            var success = await SendMessageAsync<Success, ApplySettings>(new ApplySettings() { Label = deviceName });
            Log.Debug(success.Message);
        }

        void CopyInputs(List<BitcoinTransactionInput> from, List<TxInputType> to)
        {
            foreach (var txInput in from)
            {
                to.Add(new TxInputType
                {
                    AddressNs = txInput.AddressNs,
                    Amount = txInput.Amount,
                    DecredScriptVersion = txInput.DecredScriptVersion,
                    DecredTree = txInput.DecredTree,
                    Multisig = txInput.Multisig,
                    PrevHash = txInput.PrevHash,
                    PrevIndex = txInput.PrevIndex,
                    ScriptSig = txInput.ScriptSig,
                    ScriptType = txInput.ScriptType,
                    Sequence = txInput.Sequence
                });
            }
        }

        void CopyOutputs(List<BitcoinTransactionOutput> from, List<TxOutputType> to)
        {
            foreach (var txOutput in from)
            {
                to.Add(new TxOutputType
                {
                    AddressNs = txOutput.AddressNs,
                    Address = txOutput.Address,
                    Amount = txOutput.Amount,
                    AddressType = txOutput.AddressType,
                    DecredScriptVersion = txOutput.DecredScriptVersion,
                    ExchangeType = txOutput.ExchangeType,
                    Multisig = txOutput.Multisig,
                    ScriptType = txOutput.ScriptType,
                    OpReturnData = txOutput.OpReturnData
                });
            }
        }

        public async Task<byte[]> SignTransactionAsync(BitcoinTransaction transaction)
        {
            var txDic = new Dictionary<string, TransactionType>();
            var unsignedTx = new TransactionType
            {
                Version = transaction.Version,
                InputsCnt = (uint)transaction.Inputs.Count, // must be exact number of Inputs count
                OutputsCnt = (uint)transaction.Outputs.Count, // must be exact number of Outputs count
                LockTime = transaction.LockTime,
                ExtraData = transaction.ExtraData,
                ExtraDataLen = transaction.ExtraData == null ? 0 : (uint)transaction.ExtraData.Length
            };

            CopyInputs(transaction.Inputs, unsignedTx.Inputs);
            CopyOutputs(transaction.Outputs, unsignedTx.Outputs);

            txDic.Add("unsigned", unsignedTx);

            foreach (var txInput in transaction.Inputs)
            {
                BitcoinTransaction prevTran = txInput.PrevTransaction;
                var tx = new TransactionType
                {
                    Version = prevTran.Version,
                    LockTime = prevTran.LockTime,
                    InputsCnt = (uint)prevTran.Inputs.Count,
                    OutputsCnt = (uint)prevTran.Outputs.Count,
                };
                CopyInputs(prevTran.Inputs, tx.Inputs);
                CopyOutputs(prevTran.Outputs, tx.Outputs);

                txDic.Add(txInput.PrevHash.ToHex(), tx);
            }

            var serializedTx = new Dictionary<uint, byte[]>();

            var request = await SendMessageAsync<TxRequest, SignTx>(new SignTx
            {
                CoinName = transaction.CoinName,
                Version = transaction.Version,
                LockTime = transaction.LockTime,
                Expiry = transaction.Expiry,
                InputsCount = (uint)transaction.Inputs.Count,
                OutputsCount = (uint)transaction.Outputs.Count
            });
            TxAck txAck;
            // We do loop here since we need to send over and over the same transactions to trezor because his 64 kilobytes memory
            // and he will sign chunks and return part of signed chunk in serialized manner, until we receive finall type of Txrequest TxFinished
            while (request.RequestType != RequestType.Txfinished)
            {
                TransactionType currentTx;
                if ((request.Details != null) && (request.Details.TxHash != null))
                {
                    string hash = request.Details.TxHash.ToHex();
                    if (txDic.ContainsKey(hash))
                    {
                        currentTx = txDic[hash];
                    }
                    else
                    {
                        Log.Error($"Unknown hash {hash}");
                        currentTx = txDic["unsigned"];
                    }
                }
                else
                {
                    currentTx = txDic["unsigned"];
                }

                switch (request.RequestType)
                {
                    case RequestType.Txinput:
                        {
                            var msg = new TransactionType();
                            foreach (var input in currentTx.Inputs)
                            {
                                msg.Inputs.Add(input);
                            };

                            txAck = new TxAck { Tx = msg };
                            //We send TxAck() with  TxInputs
                            request = await SendMessageAsync<TxRequest, TxAck>(txAck);

                            // Now we have to check every response is there any SerializedTx chunk 
                            if (request.Serialized != null)
                            {
                                serializedTx.Add(request.Serialized.SignatureIndex, request.Serialized.SerializedTx);
                            }

                            break;
                        }
                    case RequestType.Txoutput:
                        {
                            var msg = new TransactionType();
                            if ((request.Details != null) && (request.Details.TxHash != null))
                            {
                                foreach (var binOutput in currentTx.BinOutputs)
                                {
                                    msg.BinOutputs.Add(binOutput);
                                }
                            }
                            else
                            {
                                foreach (var output in currentTx.Outputs)
                                {
                                    msg.Outputs.Add(output);
                                }
                            }

                            txAck = new TxAck { Tx = msg };
                            //We send TxAck()  with  TxOutputs
                            request = await SendMessageAsync<TxRequest, TxAck>(txAck);

                            // Now we have to check every response is there any SerializedTx chunk 
                            if (request.Serialized != null)
                            {
                                serializedTx.Add(request.Serialized.SignatureIndex, request.Serialized.SerializedTx);
                            }

                            break;
                        }

                    case RequestType.Txextradata:
                        {
                            var offset = request.Details.ExtraDataOffset;
                            var length = request.Details.ExtraDataLen;
                            var msg = new TransactionType
                            {
                                ExtraData = currentTx.ExtraData.Skip((int)offset).Take((int)length).ToArray()
                            };
                            txAck = new TxAck { Tx = msg };
                            //We send TxAck() with  TxInputs
                            request = await SendMessageAsync<TxRequest, TxAck>(txAck);
                            // Now we have to check every response is there any SerializedTx chunk 
                            if (request.Serialized != null)
                            {
                                serializedTx.Add(request.Serialized.SignatureIndex, request.Serialized.SerializedTx);
                            }
                            break;
                        }
                    case RequestType.Txmeta:
                        {
                            var msg = new TransactionType
                            {
                                Version = currentTx.Version,
                                LockTime = currentTx.LockTime,
                                InputsCnt = currentTx.InputsCnt,
                                OutputsCnt = (request.Details != null) && (request.Details.TxHash != null) ? (uint)currentTx.BinOutputs.Count : (uint)currentTx.Outputs.Count,
                                ExtraDataLen = currentTx.ExtraData != null ? (uint)currentTx.ExtraData.Length : 0
                            };
                            txAck = new TxAck { Tx = msg };
                            //We send TxAck() with  TxInputs
                            request = await SendMessageAsync<TxRequest, TxAck>(txAck);

                            // Now we have to check every response is there any SerializedTx chunk 
                            if (request.Serialized != null)
                            {
                                serializedTx.Add(request.Serialized.SignatureIndex, request.Serialized.SerializedTx);
                            }

                            break;
                        }
                }
            }
            Log.Information($"Signed Tx :{JsonConvert.SerializeObject(serializedTx)}");
            return serializedTx.First().Value;
        }

        public async Task<EthereumTxRequest> SignEthereumTransactionAsync(EthereumSignTx signTx)
        {
            return await SendMessageAsync<EthereumTxRequest, EthereumSignTx>(signTx);
        }

    }
}
