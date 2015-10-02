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

        private volatile bool started;

        private const int DISTANCE_THRESHOLD = 15;
        private const int BACKWARD_TIME = 1000;
        private const int ROTATE_TIME = 1250;

        public MainPage()
        {
            this.InitializeComponent();
            this.Unloaded += MainPage_Unloaded;

            led = new MulticolorLed(redPinNumber: 18, greenPinNumber: 23, bluePinNumber: 24);

            button = new PushButton(pinNumber: 26);
            button.Click += Button_Click;

            distanceSensor = new Sr04UltrasonicDistanceSensor(triggerPinNumber: 12, echoPinNumber: 16);

            var motorDriver = new L298nMotorDriver(motor1Pin1: 27, motor1Pin2: 22, motor2Pin1: 5, motor2Pin2: 6);
            motors = motorDriver.AsLeftRightMotors();

            rnd = new Random(unchecked((int)(DateTime.Now.Ticks)));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            led.TurnRed();
            var loop = Task.Run(() => RobotLoop());

            base.OnNavigatedTo(e);
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
        }

        private void Button_Click(object sender, EventArgs e)
        {
            started = !started;
        }

        private async Task RobotLoop()
        {
            while (true)
            {
                if (!started)
                {
                    motors.Stop();
                    led.TurnRed();

                    Debug.WriteLine("Stopped.");
                }
                else
                {
                    if (distanceSensor.CurrentDistance < DISTANCE_THRESHOLD)
                    {
                        Debug.WriteLine("Obstacle detected. Avoiding...");
                        led.TurnBlue();

                        await motors.MoveBackwardAsync(BACKWARD_TIME);

                        if (rnd.Next(0, 2) == 0)
                            await motors.RotateLeftAsync(ROTATE_TIME);
                        else
                            await motors.RotateRightAsync(ROTATE_TIME);
                    }
                    else
                    {
                        Debug.WriteLine("Moving forward...");

                        motors.MoveForward();
                        led.TurnGreen();
                    }
                }

                await Task.Delay(150);
            }
        }
    }
}
