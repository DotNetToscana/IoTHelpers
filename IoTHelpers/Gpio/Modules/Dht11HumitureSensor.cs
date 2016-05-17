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
    public class Dht11HumitureSensor : TimedModule
    {
        private static TimeSpan DEFAULT_READ_INTERVAL = TimeSpan.FromSeconds(2);
        private const int DEFAULT_MAX_RETRIES = 20;

        private readonly Dht11 dht11;

        private double? currentTemperature;
        public double? CurrentTemperature
        {
            get
            {
                if (ReadingMode == ReadingMode.Manual)
                    throw new NotSupportedException($"{nameof(CurrentTemperature)} is available only when {nameof(ReadingMode)} is set to {ReadingMode.Continuous}.");

                return currentTemperature;
            }
        }

        private double? currentHumidity;
        public double? CurrentHumidity
        {
            get
            {
                if (ReadingMode == ReadingMode.Manual)
                    throw new NotSupportedException($"{nameof(CurrentHumidity)} is available only when {nameof(ReadingMode)} is set to {ReadingMode.Continuous}.");

                return currentHumidity;
            }
        }

        public event EventHandler ReadingChanged;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public Dht11HumitureSensor(int pinNumber, ReadingMode mode = ReadingMode.Continuous)
            : base(mode, DEFAULT_READ_INTERVAL)
        {
            var gpio = GpioController.GetDefault();
            var pin = gpio.OpenPin(pinNumber);

            dht11 = new Dht11(pin);

            base.InitializeTimer();
        }

        public async Task<Humiture> GetHumitureAsync(int maxRetries = DEFAULT_MAX_RETRIES)
        {
            var reading = await dht11.GetReadingAsync(DEFAULT_MAX_RETRIES);
            var returnValue = new Humiture(reading);

            Debug.WriteLine("Temperature: " + returnValue.Temperature);
            Debug.WriteLine("Humidity: " + returnValue.Humidity);

            return returnValue;
        }

        protected override async void OnTimer()
        {
            var humiture = await this.GetHumitureAsync();

            if (humiture.IsValid && (CurrentTemperature != humiture.Temperature || CurrentHumidity != humiture.Humidity))
            {
                currentTemperature = humiture.Temperature;
                currentHumidity = humiture.Humidity;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, ReadingChanged, RaiseEventsOnUIThread);
            }
        }

        public override void Dispose()
        {
            dht11.Dispose();
            base.Dispose();
        }
    }

    public class Humiture
    {
        public double? Humidity { get; }

        public double? Temperature { get; }

        public bool TimedOut { get; }

        public bool IsValid { get; }

        public int RetryCount { get; }

        internal Humiture(Dht11Reading reading)
        {
            IsValid = reading.IsValid;
            Humidity = reading.IsValid ? (double?)reading.Humidity : null;
            Temperature = reading.IsValid ? (double?)reading.Temperature : null;
            TimedOut = reading.TimedOut;
            RetryCount = reading.RetryCount;
        }
    }
}
