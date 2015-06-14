using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace IoTHelpers.Modules
{
    internal class PushButton2 : GpioModule
    {
        private readonly GpioPin buttonPin;

        private readonly GpioPinValue normalPinValue;
        private readonly GpioPinValue pressedPinValue;

        private GpioPinValue lastPinValue;
        private bool isPressed;

        public event EventHandler Pressed;
        public event EventHandler Released;
        public event EventHandler Click;

        private const int BUTTON_PIN = 5;

        public PushButton2(int buttonPinNumber, ButtonType type)
        {
            if (type == ButtonType.PullUp)
            {
                normalPinValue = GpioPinValue.Low;
                pressedPinValue = GpioPinValue.High;
                lastPinValue = GpioPinValue.Low;
            }
            else
            {
                normalPinValue = GpioPinValue.High;
                pressedPinValue = GpioPinValue.Low;
                lastPinValue = GpioPinValue.High;
            }

            buttonPin = Controller.OpenPin(buttonPinNumber);

            // Shows an error if the pin wasn't initialized properly
            if (buttonPin == null)
                throw new ArgumentException("There were problems initializing the GPIO button pin.");

            buttonPin.SetDriveMode(GpioPinDriveMode.Input);
            buttonPin.ValueChanged += ButtonPin_ValueChanged;
        }

        private void ButtonPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            // FallingEdge: The value of the GPIO pin changed from high to low.
            // RisingEdge: The value of the GPIO pin changed from low to high.
            System.Diagnostics.Debug.WriteLine(args.Edge);
            var currentPinValue = (args.Edge == GpioPinEdge.FallingEdge) ? GpioPinValue.Low : GpioPinValue.High;
            this.CheckButtonStatus(currentPinValue);
        }

        private void CheckButtonStatus(GpioPinValue currentPinValue)
        {
            // If same values of last read, exits.
            if (currentPinValue == lastPinValue)
                return;

            // Checks the pin value.
            if (currentPinValue == pressedPinValue)
            {
                isPressed = true;
                this.RaiseEvent(Pressed);
            }
            else if (currentPinValue == normalPinValue)
            {
                this.RaiseEvent(Released);
                if (isPressed)
                    this.RaiseEvent(Click);

                isPressed = false;
            }

            lastPinValue = currentPinValue;
        }

        private async void RaiseEvent(EventHandler eventHandler)
        {
            var dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;
            if (dispatcher != null)
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => eventHandler?.Invoke(this, EventArgs.Empty));
            else
                eventHandler?.Invoke(this, EventArgs.Empty);
        }

        public override void Dispose()
        {
            if (buttonPin != null)
            {
                buttonPin.ValueChanged -= ButtonPin_ValueChanged;
                buttonPin.Dispose();
            }
        }
    }
}
