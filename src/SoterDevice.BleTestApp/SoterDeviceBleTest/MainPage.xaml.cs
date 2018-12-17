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

        async void ButtonScan_ClickedAsync(object sender, System.EventArgs e)
        {
            await SoterDeviceFactoryBle.Instance.StartDeviceSearchAsync();
            foreach(var device in SoterDeviceFactoryBle.Instance.Devices)
            {
                Console.WriteLine($"Found device: {device.Name}");
                Device.BeginInvokeOnMainThread(async () => await device.InitializeAsync());
            }
        }
    }
}
