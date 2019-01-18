using System;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Serilog;

namespace SoterDevice.Ble
{
    public class SoterDeviceDfuBle : ISoterDeviceDfu
    {
        IDevice _device;

        const string SERVICE_GUID_STR = "0000fe59-0000-1000-8000-00805f9b34fb";
        const string DFU_PACKET_GUID_STR = "8ec90002-f315-4f60-9fb8-838830daea50";
        const string DFU_CONTROL_GUID_STR = "8ec90001-f315-4f60-9fb8-838830daea50";
        Guid _serviceGuid = new Guid(SERVICE_GUID_STR);

        public SoterDeviceDfuBle()
        {
        }

        public Task<bool> PerformDfuAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SearchAndConnectDeviceAsync()
        {
            Log.Information("Start DFU device search.");
            _device = null;
            var adapter = CrossBluetoothLE.Current.Adapter;
            adapter.DeviceDiscovered += async (s, a) =>
            {
                try
                {
                    await adapter.ConnectToDeviceAsync(a.Device);
                    Log.Verbose($"Device {a.Device.Id}({a.Device.Name})  {a.Device.State}");
                    var service = await a.Device.GetServiceAsync(_serviceGuid);
                    if ((service!=null)&&(service.Id== _serviceGuid))
                    {
                        _device = a.Device;
                        Log.Information("Connected to DFU Device.");
                    }
                    else
                    {
                        await adapter.DisconnectDeviceAsync(a.Device);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            };
            await adapter.StartScanningForDevicesAsync(new Guid[] { _serviceGuid });
            await Task.Delay(5000);
            await adapter.StopScanningForDevicesAsync();
            return _device != null;
        }
    }
}