
using Android.App;
using Android.Content.PM;
using Android.OS;
using Serilog;
using Serilog.Core;

namespace SoterDeviceBleTest.Droid
{
    [Activity(Label = "SoterDeviceBleTest", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.AndroidLog()
                .Enrich.WithProperty(Constants.SourceContextPropertyName, "Soter")
                .CreateLogger();
            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
    }
}