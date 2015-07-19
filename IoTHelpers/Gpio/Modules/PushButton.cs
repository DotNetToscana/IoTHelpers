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
    public enum ButtonType
    {
        PullDown = 0,
        PullUp = 1
    }

    public class PushButton : GpioModule
    {
        private GpioPinValue lastPinValue;

        public bool IsPressed { get; private set; } = false;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public event EventHandler Pressed;
        public event EventHandler Released;
        public event EventHandler Click;

        public PushButton(int pinNumber, ButtonType type = ButtonType.PullDown) : base(pinNumber, GpioPinDriveMode.Input, type == ButtonType.PullUp ? LogicValue.Positive : LogicValue.Negative)
        {
            lastPinValue = ActualLowPinValue;

			Pin.DebounceTimeout = TimeSpan.FromMilliseconds(20);
			Pin.ValueChanged += Pin_ValueChanged;
        }

		private void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
		{
			var currentPinValue = Pin.Read();

			// If same value of last read, exits. 
			if (currentPinValue == lastPinValue)
				return;

			// Checks the pin value.
			if (currentPinValue == ActualHighPinValue)
			{
				IsPressed = true;
				RaiseEventHelper.CheckRaiseEventOnUIThread(this, Pressed, RaiseEventsOnUIThread);
			}
			else if (currentPinValue == ActualLowPinValue)
			{
				RaiseEventHelper.CheckRaiseEventOnUIThread(this, Released, RaiseEventsOnUIThread);
				if (IsPressed)
					RaiseEventHelper.CheckRaiseEventOnUIThread(this, Click, RaiseEventsOnUIThread);

				IsPressed = false;
			}

			lastPinValue = currentPinValue;
		}

        public override void Dispose()
        {
			Pin.ValueChanged -= Pin_ValueChanged;
            base.Dispose();
        }
    }
}
