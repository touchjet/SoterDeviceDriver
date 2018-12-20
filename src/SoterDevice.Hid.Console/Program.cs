using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

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

        static async Task ResetDevice()
        {
            await SoterDeviceFactoryHid.Instance.StartDeviceSearchAsync();
            await Task.Delay(3000);
            await SoterDeviceFactoryHid.Instance.StopDeviceSearchAsync();
            if (SoterDeviceFactoryHid.Instance.Devices.Count == 0)
            {
                throw new Exception("Do Soter Wallet device detected!");
            }
            var _soterDevice = (SoterDeviceHid)SoterDeviceFactoryHid.Instance.Devices.FirstOrDefault();
            _soterDevice.EnterPinCallback = _soterDevice_EnterPinCallback; ;
            await _soterDevice.InitializeAsync();
            await _soterDevice.CancelAsync();
            if (_soterDevice.Features.Initialized)
            {
                await _soterDevice.WipeDeviceAsync();
            }
            await _soterDevice.ResetDeviceAsync("Digbig Wallet");
            await _soterDevice.ChangePinAsync();
            await _soterDevice.ChangeAutoLockDelayAsync(1200000);
            await _soterDevice.ChangeDeviceNameAsync("Test Wallet");
            var coinTable = await _soterDevice.GetCoinTable();

            Log.Information("All Done!");
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
