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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Serilog;
using SoterDevice.Contracts;
using Touchjet.BinaryUtils;

namespace SoterDevice.Ble
{
    public class SoterDeviceBle : SoterDeviceBase, IDisposable
    {
        const int PACKET_SIZE = 64;
        const int PAYLOAD_SIZE = PACKET_SIZE - 1;
        const int FIRST_CHUNK_START_INDEX = 9;

        public const string SERVICE_GUID_STR = "69996001-e8b3-11e8-9f32-f2801f1b9fd1";
        public const string DEVICE_RX_GUID_STR = "69996002-e8b3-11e8-9f32-f2801f1b9fd1";
        public const string DEVICE_TX_GUID_STR = "69996003-e8b3-11e8-9f32-f2801f1b9fd1";
        public const string DEVICE_NAME_PREFIX = "SOTW_";

        IDevice _device;
        IService _service;
        ICharacteristic _char_device_rx;
        ICharacteristic _char_device_tx;

        ConcurrentQueue<byte[]> _rxBufferQueue = new ConcurrentQueue<byte[]>();

        public SoterDeviceBle(IDevice device)
        {
            _device = device;
            Name = device.Name;
            Id = device.Id.ToString();
        }

        int _mtu = 23;
        public override int Mtu { get { return _mtu; } }

        async Task GetCharacteristicsAsync()
        {
            if (_device.State != DeviceState.Connected)
            {
                await CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(_device);
            }
            _mtu = await _device.RequestMtuAsync(255);
            if (_service != null) return;
            _service = await _device.GetServiceAsync(new Guid(SERVICE_GUID_STR));
            if (_service == null)
            {
                throw new Exception("Can not find the correct BLE service!!");
            }
            _char_device_rx = await _service.GetCharacteristicAsync(new Guid(DEVICE_RX_GUID_STR));
            if (!_char_device_rx.CanWrite)
            {
                throw new Exception("Device RX Characteristic incorrect!");
            }
            _char_device_tx = await _service.GetCharacteristicAsync(new Guid(DEVICE_TX_GUID_STR));
            if (!_char_device_tx.CanUpdate)
            {
                throw new Exception("Device TX Characteristic incorrect!");
            }
            _char_device_tx.ValueUpdated += DeviceTxValueUpdated;
            await _char_device_tx.StartUpdatesAsync();
        }

        void DeviceTxValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
        {
            _rxBufferQueue.Enqueue(e.Characteristic.Value);
        }


        protected override async Task WriteAsync(object msg)
        {
            await GetCharacteristicsAsync();
            var byteArray = Serialize(msg);

            var msgSize = (UInt32)byteArray.Length;
            var messageType = GetEnumValue("MessageType" + msg.GetType().Name);

            var msgId = (UInt16)(int)messageType;
            var data = new ByteBuffer(msgSize + PAYLOAD_SIZE, Endianness.BigEndian);
            data.PutASCII("##");
            data.Put(msgId);
            data.Put(msgSize);
            data.Put(byteArray);

            var chunks = Math.Max(data.Position, PAYLOAD_SIZE) / PAYLOAD_SIZE;

            for (var i = 0; i < chunks; i++)
            {
                var range = new byte[PACKET_SIZE];
                range[0] = (byte)'?';
                Buffer.BlockCopy(data.Value, i * PAYLOAD_SIZE, range, 1, PAYLOAD_SIZE);
                Log.Verbose($"Write to BLE: {range.ToHex()}");
                await _char_device_rx.WriteAsync(range);
            }

            _LastWrittenMessage = msg;
        }

        async Task<byte[]> GetRxBufferData()
        {
            uint retries = 0;
            byte[] result = null;
            while (retries < 600)
            {
                if (_rxBufferQueue.TryDequeue(out result))
                {
                    if (result.Length == PACKET_SIZE)
                    {
                        return result;
                    }
                }
                await Task.Delay(100);
                retries++;
            }
            throw new Exception("Error reading data from device.");
        }

        protected override async Task<object> ReadAsync()
        {
            await GetCharacteristicsAsync();
            byte[] readBuffer;

            readBuffer = await GetRxBufferData();
            Log.Verbose($"Read from HID: {readBuffer.ToHex()}");

            if (!readBuffer.Take(3).SequenceEqual(Encoding.ASCII.GetBytes("?##")))
            {
                throw new ReadException($"An error occurred while attempting to read the message from the device. The last written message was a {_LastWrittenMessage?.GetType().Name}.", readBuffer, _LastWrittenMessage);
            }

            var messageTypeInt = ((readBuffer[3] & 0xff) << 8) + readBuffer[4];

            if (!Enum.IsDefined(MessageTypeType, (int)messageTypeInt))
            {
                throw new Exception($"The number {messageTypeInt} is not a valid MessageType");
            }

            var messageTypeValueName = Enum.GetName(MessageTypeType, messageTypeInt);

            var messageType = (MessageType)Enum.Parse(MessageTypeType, messageTypeValueName);

            var remainingDataLength = ((readBuffer[5] & 0xFF) << 24)
                                      + ((readBuffer[6] & 0xFF) << 16)
                                      + ((readBuffer[7] & 0xFF) << 8)
                                      + (readBuffer[8] & 0xFF);

            var length = Math.Min(readBuffer.Length - (FIRST_CHUNK_START_INDEX), remainingDataLength);

            int dataOffset = 0;
            var allData = new byte[remainingDataLength];

            Buffer.BlockCopy(readBuffer, FIRST_CHUNK_START_INDEX, allData, dataOffset, length);
            dataOffset += length;

            remainingDataLength -= length;

            _invalidRxChunksCounter = 0;

            while (remainingDataLength > 0)
            {
                readBuffer = await GetRxBufferData();
                Log.Verbose($"Read from HID: {readBuffer.ToHex()}");

                length = Math.Min(readBuffer.Length - 1, remainingDataLength);

                if (readBuffer[0] != (byte)'?')
                {
                    if (_invalidRxChunksCounter++ > 5)
                    {
                        throw new Exception("messageRead: too many invalid chunks (2)");
                    }
                }

                Buffer.BlockCopy(readBuffer, 1, allData, dataOffset, length);
                dataOffset += length;

                if (remainingDataLength == length)
                {
                    break;
                }

                remainingDataLength -= length;
            }
            var msg = Deserialize(messageType, allData);

            return msg;
        }

        public override void Disconnect()
        {
            if (_device.State == DeviceState.Connected)
            {
                CrossBluetoothLE.Current.Adapter.DisconnectDeviceAsync(_device).Wait();
            }
        }

        public void Dispose()
        {
            Disconnect();
            _device?.Dispose();
        }
    }
}
