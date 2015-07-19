using IoTHelpers.Boards;
using IoTHelpers.Gpio.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PushButtonLed
{
    public sealed partial class MainPage : Page
	{
		private MulticolorLed led;
		private PushButton button;

		private int ledStatus = 0;

		public MainPage()
        {
            this.InitializeComponent();
			Unloaded += MainPage_Unloaded;

			led = new MulticolorLed(redPinNumber: 27, greenPinNumber: 22, bluePinNumber: 23);

			button = new PushButton(5, type: ButtonType.PullDown);
			button.Pressed += ButtonPressed;
			button.Click += ChangeColor;
			button.Released += ButtonReleased;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			var board = RaspberryPiBoard.GetDefault();
			if (board != null)
			{
				board.PowerLed.TurnOff();
				board.StatusLed.TurnOn();
			}

			base.OnNavigatedTo(e);
		}

		private void ButtonPressed(object sender, EventArgs e)
		{
			Debug.WriteLine("Button pressed");
		}

		private void ButtonReleased(object sender, EventArgs e)
		{
			Debug.WriteLine("Button released");
		}

		private void ChangeColor(object sender, EventArgs e)
		{
			this.FlipLedColor();
		}

		private void FlipLedColor()
		{
			if (ledStatus == 0)
			{
				ledStatus = 1;
				led.TurnRed();
			}
			else if (ledStatus == 1)
			{
				ledStatus = 2;
				led.TurnGreen();
			}
			else if (ledStatus == 2)
			{
				ledStatus = 3;
				led.TurnBlue();
			}
			else
			{
				ledStatus = 0;
				led.TurnOff();
			}
		}

		private void MainPage_Unloaded(object sender, object args)
		{
			// Cleanup
			if (led != null)
				led.Dispose();

			if (button != null)
			{
				button.Pressed -= ButtonPressed;
				button.Click -= ChangeColor;
				button.Released -= ButtonReleased;

				button.Dispose();
			}
		}
	}
}
