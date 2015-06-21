using IoTHelpers.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;

namespace IoTHelpers.Gpio.Modules
{
    public class InfraredReceiver : GpioModule
    {
        private readonly Timer timer;

        private readonly GpioPinValue dataReceivedPinValue;

        public event EventHandler DataReceived;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public InfraredReceiver(int pinNumber) : base(pinNumber, GpioPinDriveMode.Input)
        {
            dataReceivedPinValue = GpioPinValue.Low;

            timer = new Timer(CheckState, null, 0, 100);
        }

        private void CheckState(object state)
        {
            var currentPinValue = Pin.Read();

            // Checks the pin value.
            if (currentPinValue == dataReceivedPinValue)
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, DataReceived, RaiseEventsOnUIThread);
        }

        public override void Dispose()
        {
            timer.Dispose();
            base.Dispose();
        }
    }
}
