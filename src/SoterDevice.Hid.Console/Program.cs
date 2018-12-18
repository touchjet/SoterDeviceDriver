using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace SoterDevice.Hid.Console
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

            var _soterDevice = (SoterDeviceHid)SoterDeviceFactoryHid.Instance.Devices.FirstOrDefault();
            _soterDevice.EnterPinCallback = _soterDevice_EnterPinCallback;;
            await _soterDevice.InitializeAsync();
            var coinTable = await _soterDevice.GetCoinTable();
        }

        static Task<string> _soterDevice_EnterPinCallback()
        {
            var passStr = System.Console.ReadLine();
            return Task.FromResult(passStr);
        }

    }
}
