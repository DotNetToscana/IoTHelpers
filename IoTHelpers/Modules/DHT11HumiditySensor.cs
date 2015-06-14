using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.UI.Xaml;
using IoTHelpers.Extensions;

namespace IoTHelpers.Modules
{
    internal class DHT11HumiditySensor : GpioModule
    {
        private readonly GpioPin temperaturePin;

        private int[] dht11_dat = new int[5] { 0, 0, 0, 0, 0 };
        private const int MAX_TIMINGS = 85;

        private const int BUTTON_PIN = 6;

        public DHT11HumiditySensor(int temperaturePinNumber)
        {
            temperaturePin = Controller.OpenPin(temperaturePinNumber);

            // Shows an error if the pin wasn't initialized properly
            if (temperaturePin == null)
                throw new ArgumentException("There were problems initializing the GPIO button pin.");

            var t = new Task(ReadValues);
            t.Start();
        }

        private void ReadValues()
        {
            var timer = new Stopwatch();

            while (true)
            {
                var j = 0;
                var lastState = GpioPinValue.High;
                dht11_dat[0] = dht11_dat[1] = dht11_dat[2] = dht11_dat[3] = dht11_dat[4] = 0;

                this.SetupSensor();

                for (var i = 0; i < MAX_TIMINGS; i++)
                {
                    var counter = 0;
                    while (temperaturePin.Read() == lastState)
                    {
                        counter++;
                        //Task.Delay(TimeSpan.FromMilliseconds(.0001)).Wait();
                        timer.Restart();
                        while (timer.ElapsedMicroseconds() < 1) ;
                        if (counter == 255)
                            break;
                    }

                    lastState = temperaturePin.Read();
                    if (counter == 255)
                        break;

                    // Skip first 3 samples
                    if ((i >= 4) && (i % 2 == 0))
                    {
                        dht11_dat[j / 8] <<= 1;
                        if (counter > 16)
                            dht11_dat[j / 8] |= 1;
                        j++;
                    }
                }

                Debug.WriteLine("j: {0}", j);

                if ((j >= 40) && (dht11_dat[4] == ((dht11_dat[0] + dht11_dat[1] + dht11_dat[2] + dht11_dat[3]) & 0xFF)))
                {
                    if ((dht11_dat[0] == 0) && (dht11_dat[2] == 0))
                        return;

                    string s = string.Format("Humidity = %d.%d %% Temperature = %d.%d * C",
                        dht11_dat[0], dht11_dat[1], dht11_dat[2], dht11_dat[3]);
                }

                Task.Delay(1000).Wait();
            }
        }

        private void SetupSensor()
        {
            var timer = new Stopwatch();

            temperaturePin.SetDriveMode(GpioPinDriveMode.Output);

            temperaturePin.Write(GpioPinValue.Low);
            //Task.Delay(18).Wait();
            timer.Restart();
            while (timer.ElapsedMilliseconds < 18) ;

            temperaturePin.Write(GpioPinValue.High);
            //Task.Delay(TimeSpan.FromMilliseconds(0.040)).Wait();
            timer.Restart();
            while (timer.ElapsedMicroseconds() < 40) ;

            temperaturePin.SetDriveMode(GpioPinDriveMode.Input);
        }

        public override void Dispose()
        {
            if (temperaturePin != null)
                temperaturePin.Dispose();
        }
    }
}
