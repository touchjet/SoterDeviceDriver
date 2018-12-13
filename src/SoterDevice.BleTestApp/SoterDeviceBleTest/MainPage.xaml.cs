using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.BLE;
using Xamarin.Forms;

namespace SoterDeviceBleTest
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        async void ButtonScan_ClickedAsync(object sender, System.EventArgs e)
        {
            var ble = CrossBluetoothLE.Current;
            var adapter = CrossBluetoothLE.Current.Adapter;
            adapter.DeviceDiscovered += async (s, a) =>
            {
                try
                {
                    if ((!String.IsNullOrWhiteSpace(a.Device.Name))&&(a.Device.Name.Contains("SOTER WALLET")))
                    {
                        await adapter.ConnectToDeviceAsync(a.Device);
                        Console.WriteLine($"Device {a.Device.Id}({a.Device.Name})  {a.Device.State}");
                        foreach (var service in await a.Device.GetServicesAsync())
                        {
                            Console.WriteLine($"    Service {service.Id}({service.Name})");
                            foreach (var character in await service.GetCharacteristicsAsync())
                            {
                                Console.WriteLine($"        Character {character.Id}({character.Name}) CanRead:{character.CanRead} CanUpdate:{character.CanUpdate} CanWrite:{character.CanWrite}");
                                if (character.Id==new Guid("69996002-e8b3-11e8-9f32-f2801f1b9fd1"))
                                {
                                    await character.WriteAsync(new byte[] { 1 });
                                }
                            }
                        }
                        await adapter.DisconnectDeviceAsync(a.Device);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            };
            await adapter.StartScanningForDevicesAsync();
        }
    }
}
