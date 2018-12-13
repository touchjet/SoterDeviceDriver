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
namespace SoterDevice.Models
{
    /// <summary>
    /// BIP 004 Address Path broken up in to its constituent parts. https://github.com/bitcoin/bips/blob/master/bip-0044.mediawiki
    /// </summary>
    public interface IBIP44AddressPath : IAddressPath
    {
        /// <summary>
        /// Unhardened Purpose. This will be hardened when ToHardenedArray() is called.
        /// </summary>
        uint Purpose { get; }

        /// <summary>
        /// Unhardened CoinType. This will be hardened when ToHardenedArray() is called.
        /// </summary>
        uint CoinType { get; }

        /// <summary>
        /// Unhardened Account. This will be hardened when ToHardenedArray() is called.
        /// </summary>
        uint Account { get; }

        /// <summary>
        /// Unhardened Change. This will be not hardened when ToHardenedArray() is called.
        /// </summary>
        uint Change { get; }

        /// <summary>
        /// Unhardened AddressIndex. This will be not hardened when ToHardenedArray() is called.
        /// </summary>
        uint AddressIndex { get; }
    }
}