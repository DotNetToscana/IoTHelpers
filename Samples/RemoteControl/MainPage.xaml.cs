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
        private Dht11HumitureSensor humitureSensor;

        private RemoteConnection connection;
        private DispatcherTimer timer;

        public MainPage()
        {
            InitializeComponent();
            Unloaded += MainPage_Unloaded;

            connection = new RemoteConnection();
            connection.OnLedEvent(LedEvent);

            led = new MulticolorLed(redPinNumber: 18, greenPinNumber: 23, bluePinNumber: 24);

            humitureSensor = new Dht11HumitureSensor(pinNumber: 4);

            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, object e)
        {
            this.SendHumiture();
        }

        private void HumitureSensor_ReadingChanged(object sender, EventArgs e)
        {
            this.SendHumiture();
        }

        private void SendHumiture()
        {
            connection.SendHumiture(humitureSensor.CurrentHumidity.GetValueOrDefault(),
                humitureSensor.CurrentTemperature.GetValueOrDefault());
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

            timer.Start();
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

            if (humitureSensor != null)
            {
                humitureSensor.ReadingChanged -= HumitureSensor_ReadingChanged; 
                humitureSensor.Dispose();
            }

            if (timer != null)
            {
                timer.Tick -= Timer_Tick;
                timer.Stop();
            }

            if (connection != null)
                connection.Dispose();
        }
    }
}
