using IoTHelpers.Gpio.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using IoTHelpers.I2c.Devices;
using System.Diagnostics;

namespace Humiture
{
    public sealed partial class MainPage : Page
    {
        private readonly Dht11HumitureSensor humitureSensor;
        private readonly RemoteConnection connection;
        private readonly DispatcherTimer timer;

        public MainPage()
        {
            this.InitializeComponent();
            Unloaded += MainPage_Unloaded;

            connection = new RemoteConnection();

            humitureSensor = new Dht11HumitureSensor(pinNumber: 4)
            {
                RaiseEventsOnUIThread = true
            };
            humitureSensor.ReadingChanged += HumitureSensor_ReadingChanged;

            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            timer.Tick += Timer_Tick;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await connection.ConnectAsync();

            timer.Start();
            base.OnNavigatedTo(e);
        }

        private void HumitureSensor_ReadingChanged(object sender, EventArgs e)
        {
            var temperature = humitureSensor.CurrentTemperature;
            var humidity = humitureSensor.CurrentHumidity;

            temperatureTextBox.Text = $"{temperature}°C";
            humidityTextBox.Text = $"{humidity}%";

            SendHumiture();
        }

        private void Timer_Tick(object sender, object e) => SendHumiture();

        private async void SendHumiture()
        {
            var temperature = humitureSensor.CurrentTemperature;
            var humidity = humitureSensor.CurrentHumidity;

            if (humidity.HasValue && temperature.HasValue)
            {
                await connection.SendHumiture(humidity.Value, temperature.Value);
                Debug.WriteLine("Humidity and temperature sent to Azure.");
            }
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            // Cleanup
            if (timer != null)
            {
                timer.Tick -= Timer_Tick;
                timer.Stop();
            }

            if (humitureSensor != null)
            {
                humitureSensor.ReadingChanged -= HumitureSensor_ReadingChanged;
                humitureSensor.Dispose();
            }

            connection?.Dispose();
        }
    }
}
