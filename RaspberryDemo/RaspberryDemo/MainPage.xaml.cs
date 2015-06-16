using IoTHelpers.Modules;
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
using RaspberryDemo.Models;

namespace RaspberryDemo
{
    public sealed partial class MainPage : Page
    {
        private MulticolorLed led;
        private PushButton button;
        private ActiveBuzzer buzzer;
        private Relay relay;
        private Laser laser;

        private int ledStatus = 0;

        private SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        private SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);
        private SolidColorBrush blueBrush = new SolidColorBrush(Colors.Blue);
        private SolidColorBrush blackBrush = new SolidColorBrush(Colors.Black);

        private RemoteConnection connection;

        public MainPage()
        {
            InitializeComponent();

            Unloaded += MainPage_Unloaded;

            connection = new RemoteConnection();
            connection.OnLedEvent(LedEvent);

            led = new MulticolorLed(redPinNumber: 22, greenPinNumber: 18, bluePinNumber: 27);
            relay = new Relay(pinNumber: 24);

            button = new PushButton(pinNumber: 5);
            button.Pressed += ButtonPressed;
            button.Click += ChangeColor;
            button.Released += ButtonReleased;

            buzzer = new ActiveBuzzer(pinNumber: 12);
            laser = new Laser(pinNumber: 6);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await connection.ConnectAsync();
            connectionStatus.Text = "Connected";

            Debug.WriteLine("Program Started!");

            base.OnNavigatedTo(e);
        }

        private void LedEvent(Rgb rgb)
        {
            led.Red = rgb.Red;
            led.Green = rgb.Green;
            led.Blue = rgb.Blue;
        }

        private void ButtonPressed(object sender, EventArgs e)
        {
            buzzer.TurnOn();
        }

        private void ButtonReleased(object sender, EventArgs e)
        {
            buzzer.TurnOff();
        }

        private void ChangeColor(object sender, EventArgs e)
        {
            laser.Toogle();
            this.FlipLedColor();
        }

        private void FlipLedColor()
        {
            if (ledStatus == 0)
            {
                ledStatus = 1;
                led.TurnRed();
                LED.Fill = redBrush;
                relay.TurnOn();
            }
            else if (ledStatus == 1)
            {
                ledStatus = 2;
                led.TurnGreen();
                LED.Fill = greenBrush;
                relay.TurnOff();
            }
            else if (ledStatus == 2)
            {
                ledStatus = 3;
                led.TurnBlue();
                LED.Fill = blueBrush;
                relay.TurnOff();
            }
            else
            {
                ledStatus = 0;
                led.TurnOff();
                LED.Fill = blackBrush;
                relay.TurnOff();
            }
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            // Cleanup
            if (led != null)
                led.Dispose();

            if (button != null)
                button.Dispose();

            if (buzzer != null)
                buzzer.Dispose();

            if (relay != null)
                relay.Dispose();

            if (laser != null)
                laser.Dispose();

            if (connection != null)
                connection.Dispose();
        }
    }
}
