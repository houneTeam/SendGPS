using Xamarin.Essentials;
using Xamarin.Forms;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SendGPS
{
    public partial class MainPage : ContentPage
    {
        private const string ServerUrl = "http://192.168.4.1:5000/gps";
        private HttpClient httpClient = new HttpClient();

        public MainPage()
        {
            InitializeComponent();
            StartContinuousLocationUpdates();
        }

        private void StartContinuousLocationUpdates()
        {
            // Запускаем таймер для обновления каждые 4 секунды
            Device.StartTimer(TimeSpan.FromSeconds(4), () =>
            {
                GetAndSendLocationAsync();
                return true; // Возвращаем true, чтобы таймер продолжал работать
            });
        }

        private async void GetAndSendLocationAsync()
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();

                if (location != null)
                {
                    LocationLabel.Text = $"Latitude: {location.Latitude}, Longitude: {location.Longitude}";
                    await SendCoordinatesToServerAsync(location.Latitude, location.Longitude);
                }
                else
                {
                    LocationLabel.Text = "Unable to retrieve location.";
                }
            }
            catch (Exception ex)
            {
                LocationLabel.Text = $"Error: {ex.Message}";
            }
        }

        private async Task SendCoordinatesToServerAsync(double latitude, double longitude)
        {
            await Task.Run(async () =>
            {
                try
                {
                    // Формируем JSON-объект
                    var jsonPayload = new
                    {
                        latitude = latitude,
                        longitude = longitude
                    };
                    var jsonString = JsonConvert.SerializeObject(jsonPayload);
                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                    // Отправка POST-запроса
                    var response = await httpClient.PostAsync(ServerUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            LocationLabel.Text += "\nData sent successfully";
                        });
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            LocationLabel.Text += $"\nFailed to send data: Response Code {response.StatusCode}";
                        });
                    }
                }
                catch (Exception e)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        LocationLabel.Text += $"\nError connecting to server: {e.Message}";
                    });
                }
            });
        }
    }
}
