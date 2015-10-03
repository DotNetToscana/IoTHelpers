using IoTHelpers.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;
using IoTHelpers.Runtime;
using System.Diagnostics;

namespace IoTHelpers.Gpio.Modules
{
    public class Dht11HumitureSensor : IDisposable
    {
        private readonly Timer timer;
        private readonly Dht11 dht11;

        public double? CurrentTemperature { get; private set; }

        public double? CurrentHumidity { get; private set; }

        public event EventHandler ReadingChanged;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public Dht11HumitureSensor(int pinNumber)
        {
            var gpio = GpioController.GetDefault();
            var pin = gpio.OpenPin(pinNumber);

            dht11 = new Dht11(pin);

            timer = new Timer(CheckState, null, 0, 2000);
        }

        private async void CheckState(object state)
        {
            var temperature = CurrentTemperature;
            var humidity = CurrentHumidity;

            var reading = await dht11.GetReadingAsync();

            if (reading.IsValid)
            {
                temperature = reading.Temperature;
                humidity = reading.Humidity;
            }

            Debug.WriteLine("Temperature: " + temperature);
            Debug.WriteLine("Humidity: " + humidity);

            if (CurrentTemperature != temperature || CurrentHumidity != humidity)
            {
                CurrentTemperature = temperature;
                CurrentHumidity = humidity;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, ReadingChanged, RaiseEventsOnUIThread);
            }
        }

        public void Dispose()
        {
            timer.Dispose();
            dht11.Dispose();
        }
    }
}
