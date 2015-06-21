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
    public class MetalTouchSensor : GpioModule
    {
        private readonly Timer timer;

        private readonly GpioPinValue noTouchPinValue;
        private readonly GpioPinValue touchDetectedPinValue;

        public bool IsInContact { get; private set; } = false;

        public event EventHandler TouchDetected;
        public event EventHandler TouchRemoved;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        private List<GpioPinValue> reads = new List<GpioPinValue>(10);

        public MetalTouchSensor(int pinNumber) : base(pinNumber, GpioPinDriveMode.Input)
        {
            Pin.DebounceTimeout = TimeSpan.FromMilliseconds(10);

            noTouchPinValue = GpioPinValue.Low;
            touchDetectedPinValue = GpioPinValue.High;

            timer = new Timer(CheckState, null, 0, 20);
        }

        private void CheckState(object state)
        {
            var currentPinValue = Pin.Read();
            //System.Diagnostics.Debug.WriteLine(currentPinValue);

            if (reads.Count == 10)
            {
                if (reads.Contains(touchDetectedPinValue))
                {
                    if (!IsInContact)
                        RaiseEventHelper.CheckRaiseEventOnUIThread(this, TouchDetected, RaiseEventsOnUIThread);

                    IsInContact = true;
                }
                else if (reads.Count(r => r == noTouchPinValue) > 6)
                {
                    if (IsInContact)
                        RaiseEventHelper.CheckRaiseEventOnUIThread(this, TouchRemoved, RaiseEventsOnUIThread);

                    IsInContact = false;
                }

                reads.Clear();
            }
            else
            {
                reads.Add(currentPinValue);
            }
        }

        public override void Dispose()
        {
            timer.Dispose();
            base.Dispose();
        }
    }
}
