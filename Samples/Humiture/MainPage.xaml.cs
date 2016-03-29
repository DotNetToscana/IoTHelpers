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

namespace Humiture
{
    public sealed partial class MainPage : Page
    {
        private readonly Dht11HumitureSensor humitureSensor;

        public MainPage()
        {
            this.InitializeComponent();
            Unloaded += MainPage_Unloaded;

            humitureSensor = new Dht11HumitureSensor(pinNumber: 4);
            humitureSensor.RaiseEventsOnUIThread = true;
            humitureSensor.ReadingChanged += HumitureSensor_ReadingChanged;
        }

        private void HumitureSensor_ReadingChanged(object sender, EventArgs e)
        {
            var temperature = humitureSensor.CurrentTemperature;
            var humidity = humitureSensor.CurrentHumidity;

            temperatureTextBox.Text = $"{temperature}°C";
            humidityTextBox.Text = $"{humidity}%";
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            if (humitureSensor != null)
            {
                humitureSensor.ReadingChanged -= HumitureSensor_ReadingChanged;
                humitureSensor.Dispose();
            }
        }
    }
}
