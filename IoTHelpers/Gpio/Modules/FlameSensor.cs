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
    public class FlameSensor : GpioModule
    {
        private readonly Timer timer;

        private readonly GpioPinValue flameDetectedPinValue;
        private readonly GpioPinValue flameExtinguishedPinValue;

        private GpioPinValue lastPinValue;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public bool IsFlameDetected { get; private set; } = false;

        public event EventHandler FlameDetected;
        public event EventHandler FlameExtinguished;

        public FlameSensor(int pinNumber) : base(pinNumber, GpioPinDriveMode.Input)
        {
            flameDetectedPinValue = GpioPinValue.High;
            flameExtinguishedPinValue = GpioPinValue.Low;
            lastPinValue = flameExtinguishedPinValue;

            timer = new Timer(CheckState, null, 0, 100);
        }

        private void CheckState(object state)
        {
            var currentPinValue = Pin.Read();
            System.Diagnostics.Debug.WriteLine(currentPinValue);

            // If same value of last read, exits. 
            if (currentPinValue == lastPinValue)
                return;

            // Checks the pin value.
            if (currentPinValue == flameDetectedPinValue)
            {
                IsFlameDetected = true;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, FlameDetected, RaiseEventsOnUIThread);
            }
            else if (currentPinValue == flameExtinguishedPinValue)
            {
                IsFlameDetected = false;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, FlameExtinguished, RaiseEventsOnUIThread);
            }

            lastPinValue = currentPinValue;
        }

        public override void Dispose()
        {
            timer.Dispose();
            base.Dispose();
        }
    }
}
