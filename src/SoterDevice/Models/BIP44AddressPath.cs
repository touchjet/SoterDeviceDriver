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
namespace SoterDevice.Models
{
    public class BIP44AddressPath : AddressPathBase, IBIP44AddressPath
    {
        public uint Purpose => Validate() ? AddressPathElements[0].Value : 0;

        public uint CoinType => Validate() ? AddressPathElements[1].Value : 0;

        public uint Account => Validate() ? AddressPathElements[2].Value : 0;

        public uint Change => Validate() ? AddressPathElements[3].Value : 0;

        public uint AddressIndex => Validate() ? AddressPathElements[4].Value : 0;

        public BIP44AddressPath()
        {
        }

        public bool Validate()
        {
            var errorPrefix = "The address path is not a valid BIP44 Address Path.";
            if (AddressPathElements.Count != 5) throw new Exception($"{errorPrefix} 5 Elements are required but {AddressPathElements.Count} were found.");
            if (!AddressPathElements[0].Harden) throw new Exception($"{errorPrefix} Purpose must be hardened");
            if (AddressPathElements[0].Value != 44 && AddressPathElements[0].Value != 49) throw new Exception($"{errorPrefix} Purpose must 44 or 49");
            if (!AddressPathElements[1].Harden) throw new Exception($"{errorPrefix} Coint Type must be hardened");
            if (!AddressPathElements[2].Harden) throw new Exception($"{errorPrefix} Account must be hardened");
            if (AddressPathElements[3].Harden) throw new Exception($"{errorPrefix} Change must not be hardened");
            if (AddressPathElements[3].Value != 0 && AddressPathElements[0].Value != 1) throw new Exception($"{errorPrefix} Change must 0 or 1");
            if (AddressPathElements[4].Harden) throw new Exception($"{errorPrefix} Address Index must not be hardened");
            return true;
        }

        public BIP44AddressPath(bool isSegwit, uint coinType, uint account, bool isChange, uint addressIndex)
        {
            AddressPathElements.Add(new AddressPathElement { Value = isSegwit ? (uint)49 : 44, Harden = true });
            AddressPathElements.Add(new AddressPathElement { Value = coinType, Harden = true });
            AddressPathElements.Add(new AddressPathElement { Value = account, Harden = true });
            AddressPathElements.Add(new AddressPathElement { Value = isChange ? 1 : (uint)0, Harden = false });
            AddressPathElements.Add(new AddressPathElement { Value = addressIndex, Harden = false });
        }
    }
}
