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
        private GpioPinValue lastPinValue;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public bool IsFlameDetected { get; private set; } = false;

        public event EventHandler FlameDetected;
        public event EventHandler FlameExtinguished;

        public FlameSensor(int pinNumber, LogicValue logicValue = LogicValue.Positive) : base(pinNumber, GpioPinDriveMode.Input, logicValue)
        {
            lastPinValue = ActualLowPinValue;

            Pin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
            Pin.ValueChanged += Pin_ValueChanged;
        }

        private void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
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
            Pin.ValueChanged -= Pin_ValueChanged;
            base.Dispose();
        }
    }
}
