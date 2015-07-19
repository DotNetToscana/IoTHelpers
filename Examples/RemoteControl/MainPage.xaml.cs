using System;
using System.Collections.Generic;
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
using System.Diagnostics;
using Windows.UI;
using RemoteControl.Models;
using IoTHelpers.Gpio.Modules;
using IoTHelpers.Boards;

namespace RemoteControl
{
    public sealed partial class MainPage : Page
    {
        private MulticolorLed led;

        private RemoteConnection connection;

        public MainPage()
        {
            InitializeComponent();
            Unloaded += MainPage_Unloaded;

            connection = new RemoteConnection();
            connection.OnLedEvent(LedEvent);

            led = new MulticolorLed(redPinNumber: 27, greenPinNumber: 22, bluePinNumber: 23);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await connection.ConnectAsync();

			var board = RaspberryPiBoard.GetDefault();
			if (board != null)
			{
				board.PowerLed.TurnOff();
				board.StatusLed.TurnOn();
			}

			base.OnNavigatedTo(e);
        }

        private void LedEvent(Rgb rgb)
        {
            led.Red = rgb.Red;
            led.Green = rgb.Green;
            led.Blue = rgb.Blue;
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            // Cleanup
            if (led != null)
                led.Dispose();

            if (connection != null)
                connection.Dispose();
        }
    }
}
