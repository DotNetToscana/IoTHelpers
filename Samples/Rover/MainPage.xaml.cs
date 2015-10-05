using IoTHelpers.Gpio.Extensions;
using IoTHelpers.Gpio.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Rover
{
    public sealed partial class MainPage : Page
    {
        private readonly PushButton button;
        private readonly MulticolorLed led;
        private readonly Sr04UltrasonicDistanceSensor distanceSensor;
        private readonly LeftRightMotors motors;

        private readonly Random rnd;

        private readonly DispatcherTimer moveTimer;
        private volatile bool started;

        private const int DISTANCE_THRESHOLD_CM = 35;

        private const int BACKWARD_MIN_TIME_MS = 1000;
        private const int BACKWARD_MAX_TIME_MS = 1500;

        private const int ROTATE_MIN_TIME_MS = 1250;
        private const int ROTATE_MAX_TIME_MS = 1750;

        private const int START_DELAY_MS = 1500;
        private const int MOVE_INTERVAL_SEC = 20;

        public MainPage()
        {
            this.InitializeComponent();
            this.Unloaded += MainPage_Unloaded;

            led = new MulticolorLed(redPinNumber: 18, greenPinNumber: 23, bluePinNumber: 24);

            button = new PushButton(pinNumber: 26);
            button.RaiseEventsOnUIThread = true;
            button.Click += Button_Click;

            distanceSensor = new Sr04UltrasonicDistanceSensor(triggerPinNumber: 12, echoPinNumber: 16);

            var motorDriver = new L298nMotorDriver(motor1Pin1: 27, motor1Pin2: 22, motor2Pin1: 5, motor2Pin2: 6);
            motors = motorDriver.AsLeftRightMotors();

            rnd = new Random(unchecked((int)(DateTime.Now.Ticks)));

            moveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(MOVE_INTERVAL_SEC) };
            moveTimer.Tick += MoveTimer_Tick;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            led.TurnRed();
            var loop = Task.Run(() => RoverLoop());

            this.StartRover();

            base.OnNavigatedTo(e);
        }

        private async void StartRover()
        {
            Debug.WriteLine("Starting Rover...");

            await Task.Delay(START_DELAY_MS);

            started = true;
            moveTimer.Start();
        }

        private void StopRover()
        {
            Debug.WriteLine("Stopping Rover...");

            started = false;
            moveTimer.Stop();
        }

        private void Button_Click(object sender, EventArgs e)
        {
            if (!started)
                this.StartRover();
            else
                this.StopRover();
        }

        private void MoveTimer_Tick(object sender, object e)
        {
            this.StopRover();
        }

        private async Task RoverLoop()
        {
            while (true)
            {
                if (!started)
                {
                    if (motors.IsMoving)
                    {
                        motors.Stop();
                        led.TurnRed();

                        Debug.WriteLine("Stopped.");
                    }
                }
                else
                {
                    if (distanceSensor.CurrentDistance < DISTANCE_THRESHOLD_CM)
                    {
                        Debug.WriteLine("Obstacle detected. Avoiding...");
                        led.TurnBlue();

                        await motors.MoveBackwardAsync(rnd.Next(BACKWARD_MIN_TIME_MS, BACKWARD_MAX_TIME_MS));

                        if (rnd.Next(0, 2) == 0)
                            await motors.RotateLeftAsync(rnd.Next(ROTATE_MIN_TIME_MS, ROTATE_MAX_TIME_MS));
                        else
                            await motors.RotateRightAsync(rnd.Next(ROTATE_MIN_TIME_MS, ROTATE_MAX_TIME_MS));
                    }
                    else
                    {
                        Debug.WriteLine("Moving forward...");

                        motors.MoveForward();
                        led.TurnGreen();
                    }
                }

                await Task.Delay(100);
            }
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            started = false;

            if (button != null)
            {
                button.Click -= Button_Click;
                button.Dispose();
            }

            if (distanceSensor != null)
                distanceSensor.Dispose();

            if (led != null)
                led.Dispose();

            if (motors != null)
                motors.Dispose();

            if (moveTimer != null)
            {
                moveTimer.Stop();
                moveTimer.Tick -= MoveTimer_Tick;
            }
        }
    }
}
