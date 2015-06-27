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

        private GpioPinValue lastPinValue;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public bool IsFlameDetected { get; private set; } = false;

        public event EventHandler FlameDetected;
        public event EventHandler FlameExtinguished;

        public FlameSensor(int pinNumber, LogicValue logicValue = LogicValue.Positive) : base(pinNumber, GpioPinDriveMode.Input, logicValue)
        {
            lastPinValue = ActualLowPinValue;

            timer = new Timer(CheckState, null, 0, 100);
        }

        private void CheckState(object state)
        {
            var currentPinValue = Pin.Read();
            //System.Diagnostics.Debug.WriteLine(currentPinValue);

            // If same value of last read, exits. 
            if (currentPinValue == lastPinValue)
                return;

            // Checks the pin value.
            if (currentPinValue == ActualHighPinValue)
            {
                IsFlameDetected = true;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, FlameDetected, RaiseEventsOnUIThread);
            }
            else if (currentPinValue == ActualLowPinValue)
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
