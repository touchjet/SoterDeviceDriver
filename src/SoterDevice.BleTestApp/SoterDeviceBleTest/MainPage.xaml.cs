using System;
using System.Threading.Tasks;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
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
            if (Device.RuntimePlatform == Device.Android)
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location))
                    {
                        await DisplayAlert("Need location", "Gunna need that location", "OK");
                    }

                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
                    //Best practice to always check that the key exists
                    if (results.ContainsKey(Permission.Location))
                        status = results[Permission.Location];
                }
                if (status != PermissionStatus.Unknown)
                {
                    await DisplayAlert("Location Denied", "Can not continue, try again.", "OK");
                }
            }
            await SoterDeviceFactoryBle.Instance.StartDeviceSearchAsync();
            foreach (var device in SoterDeviceFactoryBle.Instance.Devices)
            {
                Console.WriteLine($"Found device: {device.Name}");
            }
        }

        async void Handle_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            await SoterDeviceFactoryBle.Instance.StopDeviceSearchAsync();
            var _soterDevice = (SoterDeviceBle)e.SelectedItem;
            await SoterDeviceFactoryBle.Instance.StopDeviceSearchAsync();
            await _soterDevice.InitializeAsync();
            if (_soterDevice.Features.Initialized)
            {
                await _soterDevice.WipeDeviceAsync();
            }
            await _soterDevice.ResetDeviceAsync("Digbig Wallet");
        }

        async Task<string> Device_EnterPinCallback()
        {
            await Navigation.PushModalAsync(new PinMatrixPage());
            return PinMatrixPage.pin;
        }

        async void ButtonDfu_ClickedAsync(object sender, EventArgs e)
        {
            SoterDeviceDfuBle dfuBle = new SoterDeviceDfuBle();
            await dfuBle.SearchAndConnectDeviceAsync();
        }

    }
}
