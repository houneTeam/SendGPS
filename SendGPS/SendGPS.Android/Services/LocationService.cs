using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Xamarin.Essentials;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Newtonsoft.Json;



namespace SendGPS.Droid.Services
{
    [Service]
    public class LocationService : Service
    {
        private CancellationTokenSource _cts;
        private HttpClient _httpClient = new HttpClient();
        private const string ServerUrl = "http://192.168.4.1:5000/gps";

        public override IBinder OnBind(Intent intent) => null;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => StartLocationUpdates(), _cts.Token);
            StartForeground(1, CreateNotification());
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            _cts?.Cancel();
            base.OnDestroy();
        }

        private async Task StartLocationUpdates()
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10)));

                    if (location != null)
                    {
                        await SendCoordinatesToServerAsync(location.Latitude, location.Longitude);
                    }

                    await Task.Delay(4000); // Ждем 4 секунды перед следующим обновлением
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in location updates: {ex.Message}");
            }
        }

        private async Task SendCoordinatesToServerAsync(double latitude, double longitude)
        {
            try
            {
                var jsonPayload = new { latitude, longitude };
                var jsonString = JsonConvert.SerializeObject(jsonPayload);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(ServerUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    Log.Info("LocationService", "Data sent successfully");
                }
                else
                {
                    Log.Error("LocationService", "Failed to send data");
                }
            }
            catch (Exception ex)
            {
                Log.Error("LocationService", $"Error sending data: {ex.Message}");
            }
        }


        private Notification CreateNotification()
        {
            var notificationBuilder = new Notification.Builder(this, "location_channel")
                .SetContentTitle("Location Service")
                .SetContentText("Sending location data to server")
                .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo); // Использование системной иконки

            return notificationBuilder.Build();
        }
    }
}
