using System.Collections.Generic;
using SoterDevice.Contracts;

namespace SoterDevice.Hid.Tests
{
    public class SoterCoinUtility : ICoinUtility
    {
        private readonly Dictionary<uint, CoinInfo> _CoinInfoByCoinType = new Dictionary<uint, CoinInfo>();
 
        public bool IsLegacy { get; set; } = true;
 
        public CoinInfo GetCoinInfo(uint coinNumber)
        {
            if (!_CoinInfoByCoinType.TryGetValue(coinNumber, out var retVal)) throw new DeviceException($"No coin info for coin {coinNumber}");
            return retVal;
        }

        public SoterCoinUtility(IEnumerable<CoinType> coinTypes)
        {
            foreach (var coinType in coinTypes)
            {
                var coinTypeIndex = AddressUtilities.UnhardenNumber(coinType.Bip44AccountPath);

                //Seems like there are some coins on the KeepKey with the wrong index. I.e. they are actually Ethereum?
                if (_CoinInfoByCoinType.ContainsKey(coinTypeIndex)) continue;

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

                _CoinInfoByCoinType.Add(coinTypeIndex, new CoinInfo(coinType.CoinName, addressType, !IsLegacy && coinType.Segwit, AddressUtilities.UnhardenNumber(coinType.Bip44AccountPath)));
            }
        }
    }
}
