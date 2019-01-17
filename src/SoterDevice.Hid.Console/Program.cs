using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SoterDevice.Contracts;
using SoterDevice.Models;
using Touchjet.BinaryUtils;

namespace SoterDevice.Hid
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
                        .CreateLogger();
            ResetDevice().Wait();
        }

        static ISoterDevice _soterDevice;

        static async Task ResetDevice()
        {
            await SoterDeviceFactoryHid.Instance.StartDeviceSearchAsync();
            await Task.Delay(1000);
            await SoterDeviceFactoryHid.Instance.StopDeviceSearchAsync();
            if (SoterDeviceFactoryHid.Instance.Devices.Count == 0)
            {
                throw new Exception("Do Soter Wallet device detected!");
            }
            _soterDevice = (SoterDeviceHid)SoterDeviceFactoryHid.Instance.Devices.FirstOrDefault();
            _soterDevice.EnterPinCallback = _soterDevice_EnterPinCallback; ;
            await _soterDevice.InitializeAsync();
            await _soterDevice.CancelAsync();
            if (_soterDevice.Features.Initialized)
            {
                await _soterDevice.WipeDeviceAsync();
            }
            await _soterDevice.ResetDeviceAsync("Digbig Wallet");
            await _soterDevice.InitializeAsync();
            if (!_soterDevice.Features.Initialized)
            {
                Log.Error("Reset Device Failed!!!");
                return;
            }
            await _soterDevice.ChangePinAsync();
            await _soterDevice.ChangeAutoLockDelayAsync(1200000);
            await _soterDevice.ChangeDeviceNameAsync("Test Wallet");
            var coinTable = await _soterDevice.GetCoinTable();
            _soterDevice.CoinUtility = new CoinUtility(coinTable);

            //Get Bitcoin Address
            Log.Information(await GetAddressAsync(0, false, 0, false));
            Log.Information(await GetAddressAsync(0, false, 0, false, true, false));
            Log.Information(await GetAddressAsync(0, false, 0, false, true, true));
            Log.Information(await GetAddressAsync(0, false, 0, true));

            //Get Litecoin Address
            Log.Information(await GetAddressAsync(2, false, 0, false));

            //Get Dodge Address
            Log.Information(await GetAddressAsync(3, false, 0, false));

            //Get Dash Address
            Log.Information(await GetAddressAsync(5, false, 0, false));

            //Get Ethereum Address
            Log.Information(await GetAddressAsync(60, false, 0, false));

            //Get BitcoinCash Address
            Log.Information(await GetAddressAsync(145, false, 0, false));

            //await SignBitcoinTransactionAsync();

            Log.Information("All Done!");
        }

        static async Task SignBitcoinTransactionAsync()
        {
            //get address path for address in Trezor
            var addressPath = AddressPathBase.Parse<BIP44AddressPath>("m/49'/0'/0'/0/0").ToArray();

            // previous unspent input of Transaction
            var txInput = new TxInputType()
            {
                AddressNs = addressPath,
                Amount = 100837,
                ScriptType = InputScriptType.Spendp2shwitness,
                PrevHash = "3becf448ae38cf08c0db3c6de2acb8e47acf6953331a466fca76165fdef1ccb7".ToBytes(), // transaction ID
                PrevIndex = 0,
                Sequence = 4294967293 // Sequence  number represent Replace By Fee 4294967293 or leave empty for default 
            };

            // TX we want to make a payment
            var txOut = new TxOutputType()
            {
                AddressNs = new uint[0],
                Amount = 100837,
                Address = "18UxSJMw7D4UEiRqWkArN1Lq7VSGX6qH3H",
                ScriptType = OutputScriptType.Paytoaddress // if is segwit use Spendp2shwitness

            };

            // Must be filled with basic data like below
            var signTx = new SignTx()
            {
                Expiry = 0,
                LockTime = 0,
                CoinName = "Bitcoin",
                Version = 2,
                OutputsCount = 1,
                InputsCount = 1
            };

            Log.Information($"TxSignature: {await _soterDevice.SignTransactionAsync(signTx, new List<TxInputType> { txInput }, new List<TxOutputType> { txOut })}");
        }

        static Task<string> GetAddressAsync(uint coinNumber, bool isChange, uint index, bool display, bool isPublicKey = false, bool isLegacy = true)
        {
            var coinInfo = _soterDevice.CoinUtility.GetCoinInfo(coinNumber);
            var addressPath = new BIP44AddressPath(!isLegacy && coinInfo.IsSegwit, coinNumber, 0, isChange, index);
            return _soterDevice.GetAddressAsync((IAddressPath)addressPath, isPublicKey, display);
        }

        static Task<string> _soterDevice_EnterPinCallback()
        {
            Console.WriteLine("Enter Pin Number:");
            Console.WriteLine("    7    8    9");
            Console.WriteLine("    4    5    6");
            Console.WriteLine("    1    2    3");
            var passStr = Console.ReadLine();
            return Task.FromResult(passStr);
        }

    }
}
