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
using System.Threading.Tasks;
using Serilog;
using SoterDevice.Contracts;
using HidSharp;
using Touchjet.BinaryUtils;
using System.Linq;
using System.Text;

namespace SoterDevice.Hid
{
    public class SoterDeviceHid : SoterDeviceBase, IDisposable
    {
        HidStream _hidStream;

        const int PACKET_SIZE = 64;
        const int REPORT_ID_SIZE = 1;
        const int PAYLOAD_SIZE = PACKET_SIZE - 1;
        const int FIRST_CHUNK_START_INDEX = 9;

        public const uint VID = 11044;
        public const uint PID = 1;
        public const uint HID_USAGE = 0xFF000001;

        public SoterDeviceHid(HidDevice hidDevice, string name)
        {
            if (!hidDevice.TryOpen(out _hidStream))
            {
                throw new Exception("Error open HID device");
            }
            Name = name;
            Log.Information("Soter HID Interface Connected");
        }

        protected override async Task WriteAsync(object msg)
        {
            Log.Verbose($"Write: {msg}");

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
                var range = new byte[PACKET_SIZE + REPORT_ID_SIZE];
                for (int j = 0; j < REPORT_ID_SIZE; j++)
                {
                    range[j] = 0;
                }
                range[REPORT_ID_SIZE] = (byte)'?';
                Buffer.BlockCopy(data.Value, i * PAYLOAD_SIZE, range, 1 + REPORT_ID_SIZE, PAYLOAD_SIZE);
                Log.Verbose($"Write to HID: {range.ToHex()}");
                if (!_hidStream.CanWrite)
                {
                    throw new Exception("Can't write to HID Stream!");
                }
                await _hidStream.WriteAsync(range, 0, PACKET_SIZE + REPORT_ID_SIZE);
            }

            _LastWrittenMessage = msg;
        }

        protected override async Task<object> ReadAsync()
        {
            var readBuffer = new byte[PACKET_SIZE + REPORT_ID_SIZE];

            await _hidStream.ReadAsync(readBuffer, 0, PACKET_SIZE + REPORT_ID_SIZE);
            Log.Verbose($"Read from HID: {readBuffer.ToHex()}");

            if (!readBuffer.Skip(REPORT_ID_SIZE).Take(3).SequenceEqual(Encoding.ASCII.GetBytes("?##")))
            {
                throw new ReadException($"An error occurred while attempting to read the message from the device. The last written message was a {_LastWrittenMessage?.GetType().Name}.", readBuffer, _LastWrittenMessage);
            }

            var messageTypeInt = ((readBuffer[3 + REPORT_ID_SIZE] & 0xff) << 8) + readBuffer[4 + REPORT_ID_SIZE];

            if (!Enum.IsDefined(MessageTypeType, (int)messageTypeInt))
            {
                throw new Exception($"The number {messageTypeInt} is not a valid MessageType");
            }

            var messageTypeValueName = Enum.GetName(MessageTypeType, messageTypeInt);

            var messageType = (MessageType)Enum.Parse(MessageTypeType, messageTypeValueName);

            var remainingDataLength = ((readBuffer[5 + REPORT_ID_SIZE] & 0xFF) << 24)
                                      + ((readBuffer[6 + REPORT_ID_SIZE] & 0xFF) << 16)
                                      + ((readBuffer[7 + REPORT_ID_SIZE] & 0xFF) << 8)
                                      + (readBuffer[8 + REPORT_ID_SIZE] & 0xFF);

            var length = Math.Min(readBuffer.Length - (FIRST_CHUNK_START_INDEX + REPORT_ID_SIZE), remainingDataLength);

            int dataOffset = 0;
            var allData = new byte[remainingDataLength];

            Buffer.BlockCopy(readBuffer, FIRST_CHUNK_START_INDEX + REPORT_ID_SIZE, allData, dataOffset, length);
            dataOffset += length;

            remainingDataLength -= length;

            _invalidRxChunksCounter = 0;

            while (remainingDataLength > 0)
            {
                var bytesRead = await _hidStream.ReadAsync(readBuffer, 0, PACKET_SIZE + REPORT_ID_SIZE);
                Log.Verbose($"Read from HID: {readBuffer.ToHex()}");

                if (bytesRead <= 0)
                {
                    continue;
                }

                length = Math.Min(readBuffer.Length - 1, remainingDataLength);

                if (readBuffer[REPORT_ID_SIZE] != (byte)'?')
                {
                    if (_invalidRxChunksCounter++ > 5)
                    {
                        throw new Exception("messageRead: too many invalid chunks (2)");
                    }
                }

                Buffer.BlockCopy(readBuffer, 1 + REPORT_ID_SIZE, allData, dataOffset, length);
                dataOffset += length;

                if (remainingDataLength == length)
                {
                    break;
                }

                remainingDataLength -= length;
            }

            Log.Verbose($"Message type {messageType} ({allData.Length} bytes): {allData.ToHex()}");
            var msg = Deserialize(messageType, allData);

            return msg;
        }

        public void Dispose()
        {
            _hidStream?.Close();
            _hidStream?.Dispose();
        }
    }
}
