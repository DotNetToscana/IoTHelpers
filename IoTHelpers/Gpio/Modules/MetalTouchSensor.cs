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

        private int noTouchCount = 0;

        public MetalTouchSensor(int pinNumber) : base(pinNumber, GpioPinDriveMode.Input)
        {
            noTouchPinValue = GpioPinValue.Low;
            touchDetectedPinValue = GpioPinValue.High;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(400);
            timer.Tick += CheckStatus;
            timer.Start();
        }

        private void CheckStatus(object sender, object e)
        {
            var currentPinValue = Pin.Read();
            System.Diagnostics.Debug.WriteLine(currentPinValue);

            // Normalizes the data to avoid dirty reads.
            if (currentPinValue == noTouchPinValue && IsInContact && noTouchCount++ == 6)
            {
                noTouchCount = 0;
                IsInContact = false;
                TouchRemoved?.Invoke(this, EventArgs.Empty);
            }
            else if (currentPinValue == touchDetectedPinValue)
            {
                if (noTouchCount == 0 && !IsInContact)
                    TouchDetected?.Invoke(this, EventArgs.Empty);

                noTouchCount = 0;
                IsInContact = true;
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
