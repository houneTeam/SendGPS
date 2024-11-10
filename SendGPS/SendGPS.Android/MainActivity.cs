using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
using SendGPS.Droid.Services;

namespace SendGPS.Droid
{
    [Activity(Label = "SendGPS", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel("location_channel", "Location Service", NotificationImportance.Default);
                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }

            var intent = new Intent(this, typeof(LocationService));
            StartForegroundService(intent);
        }
    }
}
