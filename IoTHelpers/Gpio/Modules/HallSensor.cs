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
    public class HallSensor : GpioModule
    {
        private GpioPinValue lastPinValue;

        public event EventHandler MagneticFieldDetected;
        public event EventHandler MagneticFieldRemoved;

        public bool IsInMagneticFieldRange { get; private set; } = false;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public HallSensor(int pinNumber, LogicValue logicValue = LogicValue.Positive) : base(pinNumber, GpioPinDriveMode.Input, logicValue)
        {
            lastPinValue = ActualLowPinValue;

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
                IsInMagneticFieldRange = true;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, MagneticFieldDetected, RaiseEventsOnUIThread);
            }
            else if (currentPinValue == ActualLowPinValue)
            {
                IsInMagneticFieldRange = false;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, MagneticFieldRemoved, RaiseEventsOnUIThread);
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
