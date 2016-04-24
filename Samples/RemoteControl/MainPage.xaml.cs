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
        private readonly MulticolorLed led;
        private readonly Dht11HumitureSensor humitureSensor;
        private readonly Relay relay;
        private readonly Sr501PirMotionDetector motionDetector;
        private readonly FlameSensor flameSensor;

        private readonly RemoteConnection connection;
        private readonly DispatcherTimer timer;

        private bool detectMovement;
        private bool detectFlame;

        public MainPage()
        {
            InitializeComponent();
            Unloaded += MainPage_Unloaded;

            //Window.Current.CoreWindow.PointerCursor = null;

            connection = new RemoteConnection();
            connection.OnLedEvent(LedEvent);

            led = new MulticolorLed(redPinNumber: 18, greenPinNumber: 23, bluePinNumber: 24);
            humitureSensor = new Dht11HumitureSensor(pinNumber: 4);
            humitureSensor.ReadingChanged += HumitureSensor_ReadingChanged;
            relay = new Relay(pinNumber: 16);

            motionDetector = new Sr501PirMotionDetector(pinNumber: 12);
            motionDetector.RaiseEventsOnUIThread = true;
            motionDetector.MotionDetected += MotionDetector_MotionDetected;
            motionDetector.MotionStopped += MotionDetector_MotionStopped;

            flameSensor = new FlameSensor(pinNumber: 27);
            flameSensor.RaiseEventsOnUIThread = true;
            flameSensor.FlameDetected += FlameSensor_FlameDetected;
            flameSensor.FlameExtinguished += FlameSensor_FlameExtinguished;

            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, object e)
            => this.SendHumiture();

        private void HumitureSensor_ReadingChanged(object sender, EventArgs e)
            => this.SendHumiture();

        private void SendHumiture()
        {
            temperatureTextBox.Text = $"{humitureSensor.CurrentTemperature}° C";
            humidityTextBox.Text = $"{humitureSensor.CurrentHumidity}%";

            connection.SendHumiture(humitureSensor.CurrentHumidity.GetValueOrDefault(),
                humitureSensor.CurrentTemperature.GetValueOrDefault());

            this.AddEvents("Temperature and humidity sent to Azure");
        }

        private void relaySwitch_Toggled(object sender, RoutedEventArgs e)
            => relay.IsOn = relaySwitch.IsOn;

        private void motionDetectorSwitch_Toggled(object sender, RoutedEventArgs e)
            => detectMovement = motionDetectorSwitch.IsOn;

        private void flameSensorSwitch_Toggled(object sender, RoutedEventArgs e)
            => detectFlame = flameSensorSwitch.IsOn;

        private void MotionDetector_MotionStopped(object sender, EventArgs e)
        {
            this.AddEvents("Motion stopped");

            led.TurnOff();
            alarm.Stop();
        }

        private void MotionDetector_MotionDetected(object sender, EventArgs e)
        {
            this.AddEvents("Motion detected");

            if (detectMovement)
            {
                led.TurnRed();
                alarm.Play();
            }
        }

        private void FlameSensor_FlameExtinguished(object sender, EventArgs e)
        {
            this.AddEvents("Flame extinguished");

            led.TurnOff();
            alarm.Stop();
        }

        private void FlameSensor_FlameDetected(object sender, EventArgs e)
        {
            this.AddEvents("Flame detected");

            if (detectFlame)
            {
                alarm.Play();
                led.TurnRed();
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await connection.ConnectAsync();

            var board = RaspberryPi2Board.GetDefault();
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
            this.AddEvents("Received Led Event from Azure.");

            led.Red = rgb.Red;
            led.Green = rgb.Green;
            led.Blue = rgb.Blue;
        }

        private void AddEvents(string message)
        {
            var dateTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            eventListBox.Items.Insert(0, $"[{dateTime}] {message}");
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            // Cleanup
            led?.Dispose();
            relay?.Dispose();

            if (humitureSensor != null)
            {
                humitureSensor.ReadingChanged -= HumitureSensor_ReadingChanged;
                humitureSensor.Dispose();
            }

            if (motionDetector != null)
            {
                motionDetector.MotionDetected -= MotionDetector_MotionDetected;
                motionDetector.MotionStopped -= MotionDetector_MotionStopped;
                motionDetector.Dispose();
            }

            if (flameSensor != null)
            {
                flameSensor.FlameDetected -= FlameSensor_FlameDetected;
                flameSensor.FlameDetected -= FlameSensor_FlameExtinguished;
                flameSensor.Dispose();
            }

            if (timer != null)
            {
                timer.Tick -= Timer_Tick;
                timer.Stop();                
            }

            connection?.Dispose();
        }
    }
}
