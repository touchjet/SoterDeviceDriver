﻿/*
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
using System.Threading.Tasks;
using SoterDevice.Contracts;
using SoterDevice.Models;

namespace SoterDevice
{
    public interface ISoterDevice
    {
        ButtonRequestHandler DeviceButtonRequestCallback { get; set; }
        EnterPinHandler EnterPinCallback { get; set; }
        ICoinUtility CoinUtility { get; set; }
        bool Connected { get; }
        string Name { get; }
        string Id { get; }
        int Mtu { get; }
        Features Features { get; }
        uint MnemonicWordCountToKeyStrength(uint wordCount);
        Task InitializeAsync();
        Task<IEnumerable<CoinType>> GetCoinTableAsync(uint maxRecords = 0);
        Task<string> GetAddressAsync(IAddressPath addressPath, bool isPublicKey, bool display);
        Task<string> GetAddressAsync(IAddressPath addressPath, bool isPublicKey, bool display, AddressType addressType, InputScriptType inputScriptType, string coinName);
        Task ResetDeviceAsync(string deviceName, uint mnemonicWordCount = 12, string language = "english");
        Task WipeDeviceAsync();
        Task CancelAsync();
        Task ChangePinAsync(bool remove = false);
        Task ChangeAutoLockDelayAsync(uint ms);
        Task ChangeDeviceNameAsync(string deviceName);
        Task<byte[]> SignTransactionAsync(BitcoinTransaction transaction);
        Task<EthereumTxRequest> SignEthereumTransactionAsync(EthereumSignTx signTx);
        void Disconnect();
    }
}
