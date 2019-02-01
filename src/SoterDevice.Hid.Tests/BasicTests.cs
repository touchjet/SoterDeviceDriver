using System;
using System.Linq;
using System.Threading.Tasks;
using HidSharp;
using Serilog;
using SoterDevice.Contracts;
using SoterDevice.Models;
using Xunit;
using Xunit.Abstractions;

namespace SoterDevice.Hid.Tests
{
    public class BasicTests
    {
        static SoterDeviceHid _soterDevice;

        public BasicTests(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.TestOutput(output, Serilog.Events.LogEventLevel.Verbose)
                .CreateLogger();
        }

        Task<string> HandleEnterPinArgs(PinMatrixRequestType pinType)
        {
            throw new Exception("Needs PIN!");
        }

        private static readonly string[] _Addresses = new string[50];

        private static async Task<string> GetAddressAsync(uint index)
        {
            return await GetAddressAsync(0, false, index, false);
        }

        private static Task<string> GetAddressAsync(uint coinNumber, bool isChange, uint index, bool display, bool isPublicKey = false, bool isLegacy = true)
        {
            var coinInfo = _soterDevice.CoinUtility.GetCoinInfo(coinNumber);
            var addressPath = new BIP44AddressPath(!isLegacy && coinInfo.IsSegwit, coinNumber, 0, isChange, index);
            return _soterDevice.GetAddressAsync((IAddressPath)addressPath, isPublicKey, display);
        }

        private async Task GetAndInitialize()
        {
            if (_soterDevice != null)
            {
                return;
            }

            await SoterDeviceFactoryHid.Instance.StartDeviceSearchAsync();
            await Task.Delay(3000);
            await SoterDeviceFactoryHid.Instance.StopDeviceSearchAsync();

            _soterDevice = (SoterDeviceHid)SoterDeviceFactoryHid.Instance.Devices.FirstOrDefault();
            _soterDevice.EnterPinCallback = HandleEnterPinArgs;
            await _soterDevice.InitializeAsync();
            var coinTable = await _soterDevice.GetCoinTableAsync();
            _soterDevice.CoinUtility = new SoterCoinUtility(coinTable);
        }

        private static async Task DoGetAddress(uint i)
        {
            var address = await GetAddressAsync(i);
            _Addresses[i] = address;
        }

        [Fact]
        public async Task DisplayBitcoinAddress()
        {
            await GetAndInitialize();
            var address = await GetAddressAsync(0, false, 0, true);
        }

        /*
        [Fact]
        public async Task GetBitcoinAddress()
        {
            await GetAndInitialize();
            var address = await GetAddressAsync(0, false, 0, false);
        }

        [Fact]
        public async Task GetBitcoinCashAddress()
        {
            await GetAndInitialize();
            var address = await GetAddressAsync(145, false, 0, false);
        }

        [Fact]
        public async Task GetBitcoinGoldAddress()
        {
            await GetAndInitialize();
            var address = await GetAddressAsync(156, false, 0, false);
        }

        [Fact]
        public async Task GetLitecoinAddress()
        {
            await GetAndInitialize();
            var address = await GetAddressAsync(2, false, 0, false);
        }

        [Fact]
        public async Task GetDashAddress()
        {
            await GetAndInitialize();
            var address = await GetAddressAsync(5, false, 0, false);
        }

        [Fact]
        public async Task GetDogeAddress()
        {
            await GetAndInitialize();
            var address = await GetAddressAsync(3, false, 0, false);
        }

        [Fact]
        public async Task DisplayDogeAddress()
        {
            await GetAndInitialize();
            var address = await GetAddressAsync(3, false, 0, true);
        }

        [Fact]
        public async Task DisplayBitcoinCashAddress()
        {
            await GetAndInitialize();
            //Coin name must be specified when displaying the address for most coins
            var address = await GetAddressAsync(145, false, 0, true);
        }

        [Fact]
        public async Task DisplayEthereumAddress()
        {
            await GetAndInitialize();
            //Ethereum coins don't need the coin name
            var address = await GetAddressAsync(60, false, 0, true);
        }

        [Fact]
        public async Task GetEthereumAddress()
        {
            await GetAndInitialize();
            //Ethereum coins don't need the coin name
            var address = await GetAddressAsync(60, false, 0, false);
        }

        [Fact]
        public async Task DisplayEthereumClassicAddress()
        {
            await GetAndInitialize();
            //Ethereum coins don't need the coin name
            var address = await GetAddressAsync(61, false, 0, true);
        }
*/
    }
}
