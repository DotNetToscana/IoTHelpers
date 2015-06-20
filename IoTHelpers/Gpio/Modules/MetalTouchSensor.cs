using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;

namespace IoTHelpers.Gpio.Modules
{
    public class MetalTouchSensor : GpioModule
    {
        private readonly DispatcherTimer timer;

        private readonly GpioPinValue noTouchPinValue;
        private readonly GpioPinValue touchDetectedPinValue;

        public bool IsInContact { get; private set; } = false;

        public event EventHandler TouchDetected;
        public event EventHandler TouchRemoved;

        private List<GpioPinValue> reads = new List<GpioPinValue>(10);

        public MetalTouchSensor(int pinNumber) : base(pinNumber, GpioPinDriveMode.Input)
        {
            Pin.DebounceTimeout = TimeSpan.FromMilliseconds(10);

            noTouchPinValue = GpioPinValue.Low;
            touchDetectedPinValue = GpioPinValue.High;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(20);
            timer.Tick += CheckStatus;
            timer.Start();
        }

        private void CheckStatus(object sender, object e)
        {
            var currentPinValue = Pin.Read();
            //System.Diagnostics.Debug.WriteLine(currentPinValue);

            if (reads.Count == 10)
            {
                if (reads.Contains(touchDetectedPinValue))
                {
                    if (!IsInContact)
                        TouchDetected?.Invoke(this, EventArgs.Empty);

                    IsInContact = true;
                }
                else if (reads.Count(r => r == noTouchPinValue) > 6)
                {
                    if (IsInContact)
                        TouchRemoved?.Invoke(this, EventArgs.Empty);

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
            timer.Stop();
            timer.Tick -= CheckStatus;

            base.Dispose();
        }
    }
}
