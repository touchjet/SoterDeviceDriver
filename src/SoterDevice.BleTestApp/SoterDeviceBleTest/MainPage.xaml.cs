using System;
using SoterDevice.Ble;
using Xamarin.Forms;

namespace SoterDeviceBleTest
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            ListViewDevice.ItemsSource = SoterDeviceFactoryBle.Instance.Devices;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        async void ButtonScan_ClickedAsync(object sender, EventArgs e)
        {
            await SoterDeviceFactoryBle.Instance.StartDeviceSearchAsync();
            foreach(var device in SoterDeviceFactoryBle.Instance.Devices)
            {
                Console.WriteLine($"Found device: {device.Name}");
            }
        }

        async void Handle_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var device = (SoterDeviceBle)e.SelectedItem;
            await SoterDeviceFactoryBle.Instance.StopDeviceSearchAsync();
            await device.InitializeAsync();
            var coinTable = await device.GetCoinTable();
        }
    }
}
