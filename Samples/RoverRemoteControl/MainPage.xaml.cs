using IoTHelpers;
using IoTHelpers.Boards;
using IoTHelpers.Gpio.Extensions;
using IoTHelpers.Gpio.Modules;
using RoverRemoteControl.Models;
using RoverRemoteControl.Services;
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
        private readonly RemoteConnection connection;
        private readonly StreamingService streamingService;
        private readonly HttpServer httpServer;

        private readonly Sr04UltrasonicDistanceSensor distanceSensor;
        private readonly LeftRightMotors motors;
        private readonly MulticolorLed led;
        private readonly PushButton button;

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

            streamingService = new StreamingService();

            httpServer = new HttpServer(1337);
            httpServer.OnRequestEventAsync(RequestEventAsync);

            connection = new RemoteConnection();
            connection.OnRoverMovementEvent(RoverMovementEvent);

            var motorDriver = new L298nMotorDriver(motor1Pin1: 27, motor1Pin2: 22, motor2Pin1: 5, motor2Pin2: 6);
            motors = motorDriver.AsLeftRightMotors();

            led = new MulticolorLed(redPinNumber: 18, greenPinNumber: 23, bluePinNumber: 24);
            distanceSensor = new Sr04UltrasonicDistanceSensor(triggerPinNumber: 12, echoPinNumber: 16, mode: ReadingMode.Continuous);

            button = new PushButton(pinNumber: 26);
            button.Click += Button_Click;

            rnd = new Random(unchecked((int)(DateTime.Now.Ticks)));
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                await streamingService.InitializeAsync();
                await streamingService.StartStreamingAsync(CameraPanel.Back, video);

                Debug.WriteLine("Camera streaming successfully started.");

                await httpServer.StartServerAsync();
                Debug.WriteLine("Web server sucsessfully started.");
            }
            catch
            { }

            try
            {
                await connection.ConnectAsync();
                led.TurnGreen();
            }
            catch
            {
                // There are connection problems.
                led.TurnRed();
            }

            var loop = Task.Run(() => RoverLoop());

            base.OnNavigatedTo(e);
        }

        private void RoverMovementEvent(RoverMovement movementData)
        {
            Debug.WriteLine(movementData.Movement.ToString());

            if (movementData.Movement == RoverMovementType.Autopilot)
            {
                // Enters autopiloting mode.
                autoPiloting = true;
            }
            else
            {
                // Otherwise, if necessary stops the autopiloting. 
                autoPiloting = false;
                led.TurnGreen();

                Action movement;
                if (movements.TryGetValue(movementData.Movement, out movement))
                    movement?.Invoke();
            }
        }

        private async Task<byte[]> RequestEventAsync()
        {
            var stream = await streamingService.GetCurrentFrameAsync();
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                return ms.ToArray();
            }
        }

        private async Task RoverLoop()
        {
            while (true)
            {
                while (autoPiloting)
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

                await Task.Delay(500);
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            if (!motors.IsMoving)
            {
                // Forces the autopitoling mode.
                autoPiloting = true;
            }
            else
            {
                // The emergency stop button has been pressed.
                autoPiloting = false;
                motors.Stop();
                led.TurnGreen();
            }
        }

        private async void MainPage_Unloaded(object sender, object args)
        {
            try
            {
                // Cleanup
                await streamingService.CleanupAsync();
                httpServer.Dispose();
                connection.Dispose();

                motors.Dispose();
                led.Dispose();
                distanceSensor.Dispose();
                button.Dispose();
            }
            catch { }
        }
    }
}
