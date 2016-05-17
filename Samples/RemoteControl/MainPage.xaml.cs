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
using Microsoft.Azure.Devices.Client;
using Windows.Devices.Geolocation;
using Newtonsoft.Json;
using System.Text;

namespace RemoteControl
{
    public sealed partial class MainPage : Page
    {
        private readonly MulticolorLed led;
        private readonly Dht11HumitureSensor humitureSensor;
        private readonly Relay relay;
        private readonly Sr501PirMotionDetector motionDetector;
        private readonly MetalTouchSensor metalTouchSensor;
        private readonly FlameSensor flameSensor;

        private readonly RemoteConnection connection;
        private readonly DispatcherTimer timer;

        private static DeviceClient deviceClient;

        private const string deviceName = "";
        private const string iotHubUri = "";
        private const string deviceKey = "";

        private Geolocator geolocator;
        private Geocoordinate position;

        private bool detectMovement;
        private bool detectTouch;
        private bool detectFlame;

        public MainPage()
        {
            InitializeComponent();
            Unloaded += MainPage_Unloaded;

            //Window.Current.CoreWindow.PointerCursor = null;

            connection = new RemoteConnection();
            connection.OnLedEvent(LedEvent);

            led = new MulticolorLed(redPinNumber: 18, greenPinNumber: 23, bluePinNumber: 25);

            humitureSensor = new Dht11HumitureSensor(pinNumber: 4);
            humitureSensor.RaiseEventsOnUIThread = true;
            humitureSensor.ReadingChanged += HumitureSensor_ReadingChanged;

            relay = new Relay(pinNumber: 16);

            motionDetector = new Sr501PirMotionDetector(pinNumber: 12);
            motionDetector.RaiseEventsOnUIThread = true;
            motionDetector.MotionDetected += MotionDetector_MotionDetected;
            motionDetector.MotionStopped += MotionDetector_MotionStopped;

            metalTouchSensor = new MetalTouchSensor(pinNumber: 5);
            metalTouchSensor.RaiseEventsOnUIThread = true;
            metalTouchSensor.TouchDetected += MetalTouchSensor_TouchDetected;
            metalTouchSensor.TouchRemoved += MetalTouchSensor_TouchRemoved;

            flameSensor = new FlameSensor(pinNumber: 26);
            flameSensor.RaiseEventsOnUIThread = true;
            flameSensor.FlameDetected += FlameSensor_FlameDetected;
            flameSensor.FlameExtinguished += FlameSensor_FlameExtinguished;

            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, object e)
        {
            this.SendHumiture();

            var temperature = humitureSensor.CurrentTemperature.GetValueOrDefault();
            var humidity = humitureSensor.CurrentHumidity.GetValueOrDefault();

            SendDeviceToCloudMessagesAsync(temperature, humidity, 0);
        }

        private void HumitureSensor_ReadingChanged(object sender, EventArgs e)
            => this.SendHumiture();

        private void SendHumiture()
        {
            var temperature = humitureSensor.CurrentTemperature.GetValueOrDefault();
            var humidity = humitureSensor.CurrentHumidity.GetValueOrDefault();

            temperatureTextBox.Text = $"{temperature}° C";
            humidityTextBox.Text = $"{humidity}%";

            connection.SendHumiture(humidity, temperature);

            this.AddEvents("Temperature and humidity sent to Azure");
        }

        private async void SendDeviceToCloudMessagesAsync(double temperature, double humidity, double lightLevel)
        {
            if (deviceClient == null || position == null)
                return;

            this.AddEvents("Sending telemetry data...");

            try
            {
                var telemetryDataPoint = new TelemetryData
                {
                    DeviceId = deviceName,
                    Temperature = Math.Round(temperature, 6),
                    Humidity = Math.Round(humidity, 6),
                    LightLevel = Math.Round(lightLevel, 6),
                    Position = new Position
                    {
                        Latitude = position.Latitude,
                        Longitude = position.Longitude,
                        Accuracy = position.Accuracy,
                        Timestamp = position.Timestamp
                    }
                };

                var messageString = JsonConvert.SerializeObject(telemetryDataPoint, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await deviceClient.SendEventAsync(message);

                this.AddEvents("Telemetry data successfully sent.");
            }
            catch (Exception ex)
            {
                this.AddEvents($"An error occured while sending data: {ex.Message}");
            }
        }

        private void relaySwitch_Toggled(object sender, RoutedEventArgs e)
            => relay.IsOn = relaySwitch.IsOn;

        private void motionDetectorSwitch_Toggled(object sender, RoutedEventArgs e)
            => detectMovement = motionDetectorSwitch.IsOn;

        private void touchSensorSwitch_Toggled(object sender, RoutedEventArgs e)
            => detectTouch = touchSensorSwitch.IsOn;

        private void flameSensorSwitch_Toggled(object sender, RoutedEventArgs e)
            => detectFlame = flameSensorSwitch.IsOn;

        private void MotionDetector_MotionDetected(object sender, EventArgs e)
        {
            this.AddEvents("Motion detected");

            if (detectMovement)
            {
                led.TurnRed();
                alarm.Play();
            }
        }

        private void MotionDetector_MotionStopped(object sender, EventArgs e)
        {
            this.AddEvents("Motion stopped");

            if (detectMovement)
            {
                led.TurnOff();
                alarm.Stop();
            }
        }

        private void MetalTouchSensor_TouchDetected(object sender, EventArgs e)
        {
            this.AddEvents("Touch detected");

            if (detectTouch)
            {
                alarm.Play();
                led.TurnRed();
            }
        }

        private void MetalTouchSensor_TouchRemoved(object sender, EventArgs e)
        {
            this.AddEvents("Touch removed");

            if (detectTouch)
            {
                led.TurnOff();
                alarm.Stop();
            }
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

        private void FlameSensor_FlameExtinguished(object sender, EventArgs e)
        {
            this.AddEvents("Flame extinguished");

            if (detectFlame)
            {
                led.TurnOff();
                alarm.Stop();
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

            if (!string.IsNullOrWhiteSpace(deviceName) && !string.IsNullOrWhiteSpace(deviceKey))
            {
                geolocator = new Geolocator();
                geolocator.PositionChanged += OnPositionChanged;

                deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceName, deviceKey),
                    TransportType.Http1);
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

        private void OnPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            position = e.Position.Coordinate;
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            // Cleanup
            if (timer != null)
            {
                timer.Tick -= Timer_Tick;
                timer.Stop();
            }

            connection?.Dispose();

            if (deviceClient != null)
            {
                geolocator.PositionChanged -= OnPositionChanged;
                geolocator = null;
                deviceClient = null;
            }

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

            if (metalTouchSensor != null)
            {
                metalTouchSensor.TouchDetected -= MetalTouchSensor_TouchRemoved;
                metalTouchSensor.TouchRemoved -= MetalTouchSensor_TouchDetected;
                metalTouchSensor.Dispose();
            }

            if (flameSensor != null)
            {
                flameSensor.FlameDetected -= FlameSensor_FlameDetected;
                flameSensor.FlameDetected -= FlameSensor_FlameExtinguished;
                flameSensor.Dispose();
            }
        }
    }
}
