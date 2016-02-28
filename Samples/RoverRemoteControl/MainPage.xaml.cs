using IoTHelpers;
using IoTHelpers.Boards;
using IoTHelpers.Gpio.Extensions;
using IoTHelpers.Gpio.Modules;
using RemoteControl;
using RoverRemoteControl.Models;
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

namespace RoverRemoteControl
{
    public sealed partial class MainPage : Page
    {
        private RemoteConnection connection;

        private readonly Sr04UltrasonicDistanceSensor distanceSensor;
        private readonly LeftRightMotors motors;
        private readonly MulticolorLed led;

        private readonly Random rnd;
        private volatile bool autoPiloting;

        private const int DISTANCE_THRESHOLD_CM = 35;

        private const int BACKWARD_MIN_TIME_MS = 1000;
        private const int BACKWARD_MAX_TIME_MS = 1500;

        private const int ROTATE_MIN_TIME_MS = 1250;
        private const int ROTATE_MAX_TIME_MS = 1750;

        private readonly Dictionary<RoverMovementType, Action> movements;

        public MainPage()
        {
            this.InitializeComponent();
            Unloaded += MainPage_Unloaded;

            connection = new RemoteConnection();
            connection.OnRoverMovementEvent(RoverMovementEvent);

            var motorDriver = new L298nMotorDriver(motor1Pin1: 27, motor1Pin2: 22, motor2Pin1: 5, motor2Pin2: 6);
            motors = motorDriver.AsLeftRightMotors();

            led = new MulticolorLed(redPinNumber: 18, greenPinNumber: 23, bluePinNumber: 24);

            movements = new Dictionary<RoverMovementType, Action>
            {
                [RoverMovementType.Forward] = () => motors.MoveForward(),
                [RoverMovementType.Backward] = () => motors.MoveBackward(),
                [RoverMovementType.TurnLeft] = () => motors.TurnLeft(),
                [RoverMovementType.TurnRight] = () => motors.TurnRight(),
                [RoverMovementType.RotateLeft] = () => motors.RotateLeft(),
                [RoverMovementType.RotateRight] = () => motors.RotateRight(),
                [RoverMovementType.Stop] = () => motors.Stop(),
            };

            distanceSensor = new Sr04UltrasonicDistanceSensor(triggerPinNumber: 12, echoPinNumber: 16, mode: ReadingMode.Manual);
            rnd = new Random(unchecked((int)(DateTime.Now.Ticks)));
        }

        private void RoverMovementEvent(RoverMovement movementData)
        {
            Debug.WriteLine(movementData.Movement.ToString());

            if (movementData.Movement == RoverMovementType.Autopilot)
            {
                // Attiva la modalità autopilota. 
                autoPiloting = true;
            }
            else
            {
                // Altrimenti, prima di eseguire il comando disattiva la modalità autopilota. 
                autoPiloting = false;
                led.TurnGreen();

                Action movement;
                if (movements.TryGetValue(movementData.Movement, out movement))
                    movement?.Invoke();
            }
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

            led.TurnGreen();
            var loop = Task.Run(() => RoverLoop());

            base.OnNavigatedTo(e);
        }

        private async Task RoverLoop()
        {
            while (true)
            {
                while (autoPiloting)
                {
                    var distance = distanceSensor.GetDistance();
                    if (distance < DISTANCE_THRESHOLD_CM)
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

        private void MainPage_Unloaded(object sender, object args)
        {
            // Cleanup
            motors?.Dispose();
            led?.Dispose();
            distanceSensor?.Dispose();
            connection?.Dispose();
        }
    }
}
