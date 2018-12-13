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
