using IoTHelpers;
using IoTHelpers.Boards;
using IoTHelpers.Gpio.Extensions;
using IoTHelpers.Gpio.Modules;
using Rover.Models;
using Rover.Services;
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
        private readonly StreamingService streamingService;
        private readonly HttpCameraServer httpCameraServer;

        private readonly RemoteConnection connection;
        private readonly LocalConnection socket;

        private readonly Sr04UltrasonicDistanceSensor distanceSensor;
        private readonly LeftRightMotors motors;
        private readonly MulticolorLed led;

        private readonly Random random;
        private volatile bool autoPiloting;

        private const int DISTANCE_THRESHOLD = 35;

        private const int BACKWARD_MIN_TIME = 1000;
        private const int BACKWARD_MAX_TIME = 1500;

        private const int ROTATE_MIN_TIME = 750;
        private const int ROTATE_MAX_TIME = 950;

        private const int AUTOPILOTING_TICK = 100;
        private const int DEFAULT_TICK = 500;

        private readonly Dictionary<RoverMovementType, Action> movements;

        public MainPage()
        {
            this.InitializeComponent();
            Unloaded += MainPage_Unloaded;

            movements = new Dictionary<RoverMovementType, Action>
            {
                [RoverMovementType.Forward] = () => motors.MoveForward(),
                [RoverMovementType.ForwardLeft] = () => motors.MoveForwardLeft(),
                [RoverMovementType.ForwardRight] = () => motors.MoveForwardRight(),
                [RoverMovementType.Backward] = () => motors.MoveBackward(),
                [RoverMovementType.BackwardLeft] = () => motors.MoveBackwardLeft(),
                [RoverMovementType.BackwardRight] = () => motors.MoveBackwardRight(),
                [RoverMovementType.RotateLeft] = () => motors.RotateLeft(),
                [RoverMovementType.RotateRight] = () => motors.RotateRight(),
                [RoverMovementType.Stop] = () => motors.Stop(),
            };

            streamingService = new StreamingService();
            httpCameraServer = new HttpCameraServer();
            httpCameraServer.OnRequestDataAsync(RequestDataAsync);

            connection = new RemoteConnection();
            connection.OnRoverMovementEvent(RoverMovementEvent);

            socket = new LocalConnection();
            socket.OnRoverMovementEvent(RoverMovementEvent);

            int motor1Pin1Number, motor1Pin2Number, motor2Pin1Number, motor2Pin2Number;
            int redPinNumber, greenPinNumber, bluePinNumber;
            int triggerPinNumber, echoPinNumber;

            switch (DeviceInformation.Type)
            {
                case DeviceType.MinnowBoardMax:
                    motor1Pin1Number = 0;
                    motor1Pin2Number = 1;
                    motor2Pin1Number = 2;
                    motor2Pin2Number = 3;
                    redPinNumber = 4;
                    greenPinNumber = 5;
                    bluePinNumber = 6;
                    triggerPinNumber = 7;
                    echoPinNumber = 8;

                    break;

                case DeviceType.Colibri:
                    motor1Pin1Number = 98;
                    motor1Pin2Number = 103;
                    motor2Pin1Number = 97;
                    motor2Pin2Number = 79;

                    // On Colibri there's not enough Pin on to be used only via GPIO: don't use Multicolor LED.
                    redPinNumber = greenPinNumber = bluePinNumber = -1;

                    triggerPinNumber = 133;
                    echoPinNumber = 101;

                    break;

                case DeviceType.RaspberryPi2:
                case DeviceType.RaspberryPi3:
                default:
                    motor1Pin1Number = 27;
                    motor1Pin2Number = 22;
                    motor2Pin1Number = 5;
                    motor2Pin2Number = 6;
                    redPinNumber = 18;
                    greenPinNumber = 23;
                    bluePinNumber = 24;
                    triggerPinNumber = 12;
                    echoPinNumber = 16;

                    break;
            }

            var motorDriver = new L298nMotorDriver(motor1Pin1Number, motor1Pin2Number, motor2Pin1Number, motor2Pin2Number);
            motors = motorDriver.AsLeftRightMotors();

            distanceSensor = new Sr04UltrasonicDistanceSensor(triggerPinNumber, echoPinNumber, mode: ReadingMode.Continuous);

            // Checks whether the Pin number for Multicolor LED has been specified.
            if (redPinNumber > 0)
                led = new MulticolorLed(redPinNumber, greenPinNumber, bluePinNumber);

            random = new Random(unchecked((int)(DateTime.Now.Ticks)));
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                await streamingService.InitializeAsync();
                await streamingService.StartStreamingAsync(CameraPanel.Back, video);

                if (streamingService.CurrentState == ScenarioState.Streaming)
                {
                    Debug.WriteLine("Camera streaming successfully started.");

                    await httpCameraServer.StartServerAsync();
                    Debug.WriteLine("Web server sucsessfully started.");
                }
            }
            catch
            {
                Debug.WriteLine("Unable to start camera streaming.");
            }

            try
            {
                await socket.StartServerAsync();
                Debug.WriteLine("Successfully started local socket.");

                await connection.ConnectAsync();
                led?.TurnGreen();

                Debug.WriteLine("Successfully connected to SignalR endpoint.");
            }
            catch (Exception ex)
            {
                // There are connection problems.
                led?.TurnRed();

                Debug.WriteLine($"A connection problem occoured: {ex.Message}");
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
                led?.TurnGreen();

                Action movement;
                if (movements.TryGetValue(movementData.Movement, out movement))
                    movement?.Invoke();
            }
        }

        private async Task<byte[]> RequestDataAsync()
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
                if (autoPiloting)
                {
                    if (distanceSensor.CurrentDistance < DISTANCE_THRESHOLD)
                    {
                        Debug.WriteLine("Obstacle detected. Avoiding...");
                        led?.TurnBlue();

                        await motors.MoveBackwardAsync(random.Next(BACKWARD_MIN_TIME, BACKWARD_MAX_TIME));

                        var movementInterval = random.Next(ROTATE_MIN_TIME, ROTATE_MAX_TIME);
                        if ((random.Next(0, 11) % 2) == 0)
                            await motors.RotateLeftAsync(movementInterval);
                        else
                            await motors.RotateRightAsync(movementInterval);
                    }
                    else
                    {
                        Debug.WriteLine("Moving forward...");

                        motors.MoveForward();
                        led?.TurnGreen();
                    }

                    await Task.Delay(AUTOPILOTING_TICK);
                }
                else
                {
                    await Task.Delay(DEFAULT_TICK);
                }
            }
        }

        private async void MainPage_Unloaded(object sender, object args)
        {
            try
            {
                // Cleanup
                await streamingService.CleanupAsync();
                httpCameraServer.Dispose();
                connection.Dispose();
                socket.Dispose();

                motors.Dispose();
                led?.Dispose();
                distanceSensor.Dispose();
            }
            catch { }
        }
    }
}
