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
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtoBuf;
using Serilog;
using SoterDevice.Contracts;
using SoterDevice.Models;
using Touchjet.BinaryUtils;

namespace SoterDevice
{
    public abstract class SoterDeviceBase : ISoterDevice
    {
        public EnterPinArgs EnterPinCallback;

        SemaphoreSlim _Lock = new SemaphoreSlim(1, 1);
        static readonly Dictionary<string, Type> _ContractsByName = new Dictionary<string, Type>();

        protected object _LastWrittenMessage;

        protected abstract Task WriteAsync(object msg);

        protected abstract Task<object> ReadAsync();

        protected Type MessageTypeType => typeof(MessageType);

        protected int _invalidRxChunksCounter;

        public bool IsInitialized => Features != null;

        public Features Features { get; private set; }

        public string Name { get; protected set; }

        public ICoinUtility CoinUtility { get; set; }

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

        public async Task<TReadMessage> SendMessageAsync<TReadMessage, TWriteMessage>(TWriteMessage message)
        {
            ValidateInitialization(message);
            Log.Debug($"Message --> {typeof(TWriteMessage).ToString().Substring(22)} {JsonConvert.SerializeObject(message)}");

            await _Lock.WaitAsync();

            try
            {
                var response = await SendMessageAsync(message);
                Log.Debug($"Message --> {typeof(TReadMessage).ToString().Substring(22)} {JsonConvert.SerializeObject(response)}");

                for (var i = 0; i < 10; i++)
                {
                    if (IsPinMatrixRequest(response))
                    {
                        var pin = await EnterPinCallback.Invoke();
                        response = await PinMatrixAckAsync(pin);
                        if (response is TReadMessage readMessage)
                        {
                            return readMessage;
                        }
                    }

                    else if (IsButtonRequest(response))
                    {
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
            await WriteAsync(message);

            var retVal = await ReadAsync();

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
                throw new FailureException<Failure>("PIN Attempt Failed.", failure);
            }

            return retVal;
        }

        bool IsPinMatrixRequest(object response)
        {
            return response is PinMatrixRequest;
        }

        async Task<object> PinMatrixAckAsync(string pin)
        {
            var retVal = await SendMessageAsync(new PinMatrixAck { Pin = pin });

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
                throw new Exception("Error initializing KeepKey. Features were not retrieved");
            }
        }

        public async Task<IEnumerable<CoinType>> GetCoinTable()
        {
            var coinInfos = new List<CoinType>();
            var coinTable = await SendMessageAsync<CoinTable, GetCoinTable>(new GetCoinTable());

            uint coinIndex = 0;
            uint numOfCoins = coinTable.NumCoins;
            uint chunkSize = coinTable.ChunkSize;
            while (coinIndex <numOfCoins)
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

        public async Task ResetDeviceAsync(string deviceName, uint strength = 128, string language="english")
        {
            var entropyRequest = await SendMessageAsync<EntropyRequest, ResetDevice>(new ResetDevice { DisplayRandom=false,Strength=strength, PassphraseProtection=false, PinProtection=true, Language=language, Label=deviceName});
        }
    }
}
