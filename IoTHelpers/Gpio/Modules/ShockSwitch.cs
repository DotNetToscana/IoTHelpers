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
    public class ShockSwitch : GpioModule
    {
        private GpioPinValue lastPinValue;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public event EventHandler Shaked;

        public ShockSwitch(int pinNumber, LogicValue logicValue = LogicValue.Negative) : base(pinNumber, GpioPinDriveMode.Input, logicValue)
        {
            lastPinValue = ActualLowPinValue;

            Pin.ValueChanged += Pin_ValueChanged;
        }

        private void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            var currentPinValue = Pin.Read();

            // Checks the pin value.
            if (currentPinValue != lastPinValue && currentPinValue == ActualHighPinValue)
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, Shaked, RaiseEventsOnUIThread);

            lastPinValue = currentPinValue;
        }

        public override void Dispose()
        {
            Pin.ValueChanged -= Pin_ValueChanged;
            base.Dispose();
        }
    }
}
