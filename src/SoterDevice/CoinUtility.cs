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
using SoterDevice.Contracts;
using SoterDevice.Models;

namespace SoterDevice
{
    public class CoinUtility : ICoinUtility
    {
        public CoinUtility(IEnumerable<CoinType> coinTypes)
        {
            foreach (var coinType in coinTypes)
            {
                var coinTypeIndex = AddressUtilities.UnhardenNumber(coinType.Bip44AccountPath);

                //Seems like there are some coins on the KeepKey with the wrong index. I.e. they are actually Ethereum?
                if (Coins.ContainsKey(coinTypeIndex)) continue;

                AddressType addressType;

                switch (coinType.AddressType)
                {
                    case 65535:
                        addressType = AddressType.Ethereum;
                        break;
                    default:
                        addressType = AddressType.Bitcoin;
                        break;
                }

                Coins.Add(coinTypeIndex, new CoinInfo(coinType.CoinName, addressType, !IsLegacy && coinType.Segwit, AddressUtilities.UnhardenNumber(coinType.Bip44AccountPath)));
            }
        }

        public bool IsLegacy { get; set; } = true;

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
