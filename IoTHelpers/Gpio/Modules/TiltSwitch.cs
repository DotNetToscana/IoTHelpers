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
    public class TiltSwith : GpioModule
    {
        private readonly Timer timer;

        private GpioPinValue lastPinValue;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public event EventHandler Tilt;

        public TiltSwith(int pinNumber, LogicValue logicValue = LogicValue.Negative) : base(pinNumber, GpioPinDriveMode.Input, logicValue)
        {
            lastPinValue = ActualHighPinValue;

            timer = new Timer(CheckState, null, 0, 40);
        }

        private void CheckState(object state)
        {
            var currentPinValue = Pin.Read();

            // Checks the pin value.
            if (currentPinValue != lastPinValue && currentPinValue == ActualHighPinValue)
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, Tilt, RaiseEventsOnUIThread);

            lastPinValue = currentPinValue;
        }

        public override void Dispose()
        {
            timer.Dispose();
            base.Dispose();
        }
    }
}
