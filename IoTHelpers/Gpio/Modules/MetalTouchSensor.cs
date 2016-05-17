using IoTHelpers.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace IoTHelpers.Gpio.Modules
{
    public class MetalTouchSensor : GpioModule
    {
        private GpioPinValue lastPinValue;

        public bool IsInContact { get; private set; } = false;

        public event EventHandler TouchDetected;
        public event EventHandler TouchRemoved;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public MetalTouchSensor(int pinNumber, LogicValue logicValue = LogicValue.Positive) : base(pinNumber, GpioPinDriveMode.Input, logicValue)
        {
            Pin.DebounceTimeout = TimeSpan.FromMilliseconds(10);
            Pin.ValueChanged += Pin_ValueChanged;
        }

        private void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            var currentPinValue = Pin.Read();
            //Debug.WriteLine(currentPinValue);

            // If same value of last read, exits. 
            if (currentPinValue == lastPinValue)
                return;

            // Checks the pin value.
            if (currentPinValue == ActualHighPinValue)
            {
                IsInContact = true;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, TouchDetected, RaiseEventsOnUIThread);
            }
            else if (currentPinValue == ActualLowPinValue)
            {
                IsInContact = false;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, TouchRemoved, RaiseEventsOnUIThread);
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
