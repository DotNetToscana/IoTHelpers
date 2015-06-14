using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;

namespace IoTHelpers.Modules
{
    public class TiltSwith : GpioModule
    {
        private readonly DispatcherTimer timer;

        private readonly GpioPinValue tiltedPinValue;

        private GpioPinValue lastPinValue;    

        public event EventHandler Tilt;

        public TiltSwith(int pinNumber) : base(pinNumber)
        {
            tiltedPinValue = GpioPinValue.Low;
            lastPinValue = GpioPinValue.High;

            Pin.SetDriveMode(GpioPinDriveMode.Input);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += CheckStatus;
            timer.Start();
        }

        private void CheckStatus(object sender, object e)
        {
            var currentPinValue = Pin.Read();

            // If same values of last read, exits. 
            if (currentPinValue == lastPinValue)
                return;

            // Checks the pin value.
            if (currentPinValue == tiltedPinValue)
                Tilt?.Invoke(this, EventArgs.Empty);

            lastPinValue = currentPinValue;
        }

        public override void Dispose()
        {
            timer.Stop();
            timer.Tick -= CheckStatus;

            if (Pin != null)
                Pin.Dispose();
        }
    }
}
