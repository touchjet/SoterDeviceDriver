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

namespace SoterDevice
{
    public class CoinUtility
    {
        public CoinUtility()
        {
            Coins.Add(0, new CoinInfo("Bitcoin", AddressType.Bitcoin, true, 0));
            Coins.Add(1, new CoinInfo("Testnet", AddressType.Bitcoin, true, 1));
            Coins.Add(2, new CoinInfo("Litecoin", AddressType.Bitcoin, true,2));
            Coins.Add(3, new CoinInfo("Dogecoin", AddressType.Bitcoin, false, 3));

            Coins.Add(60, new CoinInfo("Ethereum", AddressType.Ethereum, false, 60));
            Coins.Add(61, new CoinInfo("Ethereum Classic", AddressType.Ethereum, false, 61));
        }

        public Dictionary<uint, CoinInfo> Coins { get; } = new Dictionary<uint, CoinInfo>();

        public CoinInfo GetCoinInfo(uint coinNumber)
        {
            if (Coins.TryGetValue(coinNumber, out var coinInfo))
            {
                return coinInfo;
            }

            throw new NotImplementedException($"The coin number {coinNumber} has not been filled. Please create a class that implements ICoinUtility, or call an overload that specifies coin information.");
        }
    }
}
