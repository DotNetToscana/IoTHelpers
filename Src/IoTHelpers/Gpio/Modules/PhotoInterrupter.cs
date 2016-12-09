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
    public class PhotoInterrupter : GpioModule
    {
        private readonly Timer timer;

        private GpioPinValue lastPinValue;

        public bool PhotoPassThrough { get; private set; } = true;

        public event EventHandler Interrupted;
        public event EventHandler PassThrough;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public PhotoInterrupter(int pinNumber, LogicValue logicValue = LogicValue.Positive) : base(pinNumber, GpioPinDriveMode.Input, logicValue)
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
                PhotoPassThrough = false;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, Interrupted, RaiseEventsOnUIThread);
            }
            else if (currentPinValue == ActualLowPinValue)
            {
                PhotoPassThrough = true;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, PassThrough, RaiseEventsOnUIThread);
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
