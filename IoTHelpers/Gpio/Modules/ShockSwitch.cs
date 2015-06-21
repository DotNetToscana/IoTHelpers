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
        private readonly GpioPinValue shockedPinValue;

        private GpioPinValue lastPinValue;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public event EventHandler Shaked;

        public ShockSwitch(int pinNumber) : base(pinNumber, GpioPinDriveMode.Input)
        {
            shockedPinValue = GpioPinValue.Low;
            lastPinValue = GpioPinValue.High;

            Pin.ValueChanged += Pin_ValueChanged;
        }

        private void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            var currentPinValue = Pin.Read();

            // Checks the pin value.
            if (currentPinValue != lastPinValue && currentPinValue == shockedPinValue)
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
