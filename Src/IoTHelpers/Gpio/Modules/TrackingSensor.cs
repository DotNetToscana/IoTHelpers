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

namespace IoTHelpers.Gpio.Modules
{
    public class TrackingSensor : GpioModule
    {
        private GpioPinValue lastPinValue;

        public bool IsWhiteLineDetected { get; private set; } = false;

        public event EventHandler WhiteLineDetected;
        public event EventHandler BlackLineDetected;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public TrackingSensor(int pinNumber, LogicValue logicValue = LogicValue.Negative) : base(pinNumber, GpioPinDriveMode.Input, logicValue)
        {
            lastPinValue = ActualLowPinValue;

            Pin.ValueChanged += Pin_ValueChanged;
        }

        private void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            var currentPinValue = Pin.Read();
            System.Diagnostics.Debug.WriteLine(currentPinValue);

            // If same value of last read, exits. 
            if (currentPinValue == lastPinValue)
                return;

            // Checks the pin value.
            if (currentPinValue == ActualHighPinValue)
            {
                IsWhiteLineDetected = true;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, WhiteLineDetected, RaiseEventsOnUIThread);
            }
            else if (currentPinValue == ActualLowPinValue)
            {
                IsWhiteLineDetected = false;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, BlackLineDetected, RaiseEventsOnUIThread);
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
