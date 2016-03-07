using IoTHelpers.Boards;
using IoTHelpers.Gpio.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Gpio;
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
    /* Without IoTHelpers library

    public sealed partial class MainPage : Page
    {
        private GpioPin redPin, greenPin, bluePin;
        private GpioPin button;

        public MainPage()
        {
            this.InitializeComponent();
            Unloaded += MainPage_Unloaded;

            var gpio = GpioController.GetDefault();

            redPin = gpio.OpenPin(pinNumber: 18);
            redPin.Write(GpioPinValue.Low);
            redPin.SetDriveMode(GpioPinDriveMode.Output);

            greenPin = gpio.OpenPin(pinNumber: 23);
            greenPin.Write(GpioPinValue.Low);
            greenPin.SetDriveMode(GpioPinDriveMode.Output);

            bluePin = gpio.OpenPin(pinNumber: 25);
            bluePin.Write(GpioPinValue.Low);
            bluePin.SetDriveMode(GpioPinDriveMode.Output);

            button = gpio.OpenPin(pinNumber: 16);
            button.SetDriveMode(GpioPinDriveMode.Input);
            button.ValueChanged += Button_ValueChanged;
        }

        private void Button_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            var currentPinValue = button.Read();

            if (currentPinValue == GpioPinValue.Low)
            {
                redPin.Write(GpioPinValue.High);
            }
            else if (currentPinValue == GpioPinValue.High)
            {
                redPin.Write(GpioPinValue.Low);
            }
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            redPin.Dispose();
            greenPin.Dispose();
            bluePin.Dispose();

            button.ValueChanged -= Button_ValueChanged;
            button.Dispose();
        }
    }
    */

    public sealed partial class MainPage : Page
    {
        private readonly MulticolorLed led;
        private readonly PushButton button;

        public MainPage()
        {
            this.InitializeComponent();
            Unloaded += MainPage_Unloaded;

            led = new MulticolorLed(redPinNumber: 18, greenPinNumber: 23, bluePinNumber: 25);

            button = new PushButton(pinNumber: 16);
            button.Pressed += ButtonPressed;
            button.Released += ButtonReleased;
        }

        private void ButtonPressed(object sender, EventArgs e)
            => led.TurnRed();

        private void ButtonReleased(object sender, EventArgs e)
            => led.TurnOff();

        private void MainPage_Unloaded(object sender, object args)
        {
            led.Dispose();

            button.Pressed -= ButtonPressed;
            button.Released -= ButtonReleased;
            button.Dispose();
        }
    }

    /* Flip sample

    public sealed partial class MainPage : Page
    {
        private readonly MulticolorLed led;
        private readonly PushButton button;

        private int ledStatus = 0;

        public MainPage()
        {
            this.InitializeComponent();
            Unloaded += MainPage_Unloaded;

            led = new MulticolorLed(redPinNumber: 18, greenPinNumber: 23, bluePinNumber: 25);

            button = new PushButton(pinNumber: 16);
            button.Pressed += ButtonPressed;
            button.Released += ButtonReleased;
            button.Click += FlipLedColor;
        }

        private void ButtonPressed(object sender, EventArgs e) =>
            Debug.WriteLine("Button pressed");

        private void ButtonReleased(object sender, EventArgs e)
            => Debug.WriteLine("Button released");

        private void FlipLedColor(object sender, EventArgs e)
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
            led?.Dispose();

            if (button != null)
            {
                button.Pressed -= ButtonPressed;
                button.Released -= ButtonReleased;
                button.Click -= FlipLedColor;

                button.Dispose();
            }
        }
    }
    */
}
