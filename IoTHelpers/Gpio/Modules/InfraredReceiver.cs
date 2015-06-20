using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;

namespace IoTHelpers.Gpio.Modules
{
    public class InfraredReceiver : GpioModule
    {
        private readonly DispatcherTimer timer;

        private readonly GpioPinValue dataReceivedPinValue;

        public event EventHandler DataReceived;

        public InfraredReceiver(int pinNumber) : base(pinNumber, GpioPinDriveMode.Input)
        {
            dataReceivedPinValue = GpioPinValue.Low;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += CheckStatus;
            timer.Start();
        }

        private void CheckStatus(object sender, object e)
        {
            var currentPinValue = Pin.Read();

            // Checks the pin value.
            if (currentPinValue == dataReceivedPinValue)
                DataReceived?.Invoke(this, EventArgs.Empty);
        }

        public override void Dispose()
        {
            timer.Stop();
            timer.Tick -= CheckStatus;

            base.Dispose();
        }
    }
}
