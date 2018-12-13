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
using System.Linq;
namespace SoterDevice.Models
{
    public abstract class AddressPathBase : IAddressPath
    {
        #region Public Properties
        public List<IAddressPathElement> AddressPathElements { get; private set; } = new List<IAddressPathElement>();
        #endregion

        #region Private Static Methods
        private static AddressPathElement ParseElement(string elementString)
        {
            if (!uint.TryParse(elementString.Replace("'", string.Empty), out var unhardenedNumber))
            {
                throw new Exception($"The value {elementString} is not a valid path element");
            }

            return new AddressPathElement { Harden = elementString.EndsWith("'"), Value = unhardenedNumber };
        }
        #endregion

        #region Public Methods
        public uint[] ToArray() => AddressPathElements.Select(ape => ape.Harden ? AddressUtilities.HardenNumber(ape.Value) : ape.Value).ToArray();
        #endregion

        public override string ToString()
        {
            return $"m/{string.Join("/", AddressPathElements.Select(ape => $"{ape.Value}{(ape.Harden ? "'" : string.Empty)}"))}";
        }

        #region Public Static Methods
        public static T Parse<T>(string path) where T : AddressPathBase, new() =>
            new T
            {
                AddressPathElements = path.Split('/')
               .Where(t => string.Compare("m", t, StringComparison.OrdinalIgnoreCase) != 0)
               .Select(ParseElement)
               .Cast<IAddressPathElement>()
               .ToList()
            };
        #endregion
    }
}
