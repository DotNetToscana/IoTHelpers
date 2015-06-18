using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;

namespace IoTHelpers.Gpio.Modules
{
    public enum ButtonType
    {
        PullDown = 0,
        PullUp = 1
    }

    public class PushButton : GpioModule
    {
        private readonly DispatcherTimer timer;

        private readonly GpioPinValue normalPinValue;
        private readonly GpioPinValue pressedPinValue;

        private GpioPinValue lastPinValue;

        public bool IsPressed { get; private set; } = false;

        public event EventHandler Pressed;
        public event EventHandler Released;
        public event EventHandler Click;

        public PushButton(int pinNumber, ButtonType type = ButtonType.PullDown) : base(pinNumber, GpioPinDriveMode.Input)
        {
            if (type == ButtonType.PullUp)
            {
                normalPinValue = GpioPinValue.Low;
                pressedPinValue = GpioPinValue.High;
            }
            else
            {
                normalPinValue = GpioPinValue.High;
                pressedPinValue = GpioPinValue.Low;
            }

            lastPinValue = normalPinValue;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += CheckButtonStatus;
            timer.Start();
        }

        private void CheckButtonStatus(object sender, object e)
        {
            var currentPinValue = Pin.Read();

            // If same value of last read, exits. 
            if (currentPinValue == lastPinValue)
                return;

            // Checks the pin value.
            if (currentPinValue == pressedPinValue)
            {
                IsPressed = true;
                Pressed?.Invoke(this, EventArgs.Empty);
            }
            else if (currentPinValue == normalPinValue)
            {
                Released?.Invoke(this, EventArgs.Empty);
                if (IsPressed)
                    Click?.Invoke(this, EventArgs.Empty);

                IsPressed = false;
            }

            lastPinValue = currentPinValue;
        }

        public override void Dispose()
        {
            timer.Stop();
            timer.Tick -= CheckButtonStatus;

            base.Dispose();
        }
    }
}
