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
    public class Sr501PirMotionDetector : GpioModule
    {
        private GpioPinValue lastPinValue;

        public event EventHandler MotionDetected;
        public event EventHandler MotionStopped;

        private bool IsMotionDetected { get; set; }

        public bool RaiseEventsOnUIThread { get; set; } = false; 

        public Sr501PirMotionDetector(int pinNumber, LogicValue logicValue = LogicValue.Positive) : base(pinNumber, GpioPinDriveMode.Input, logicValue)
        {
            lastPinValue = ActualLowPinValue;

            Pin.DebounceTimeout = TimeSpan.FromMilliseconds(20);
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
                IsMotionDetected = true; 
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, MotionDetected, RaiseEventsOnUIThread);
            }
            else if (currentPinValue == ActualLowPinValue)
            {
                IsMotionDetected = false;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, MotionStopped, RaiseEventsOnUIThread);
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
