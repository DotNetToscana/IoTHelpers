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

        private readonly LeftRightMotors motors;
        private readonly MulticolorLed led;

        public MainPage()
        {
            this.InitializeComponent();
            Unloaded += MainPage_Unloaded;

            connection = new RemoteConnection();
            connection.OnRoverMovementEvent(RoverMovementEvent);

            var motorDriver = new L298nMotorDriver(motor1Pin1: 27, motor1Pin2: 22, motor2Pin1: 5, motor2Pin2: 6);
            motors = motorDriver.AsLeftRightMotors();

            led = new MulticolorLed(redPinNumber: 18, greenPinNumber: 23, bluePinNumber: 24);
        }

        private void RoverMovementEvent(RoverMovement movementData)
        {
            Debug.WriteLine(movementData.Movement.ToString());

            switch (movementData.Movement)
            {
                case RoverMovementType.Forward:                    
                    motors.MoveForward();
                    break;

                case RoverMovementType.Backward:
                    motors.MoveBackward();
                    break;

                case RoverMovementType.TurnLeft:
                    motors.TurnLeft();
                    break;

                case RoverMovementType.TurnRight:
                    motors.TurnRight();
                    break;

                case RoverMovementType.RotateLeft:
                    motors.RotateLeft();
                    break;

                case RoverMovementType.RotateRight:
                    motors.RotateRight();
                    break;

                case RoverMovementType.Stop:
                default:
                    motors.Stop();
                    break;
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
            base.OnNavigatedTo(e);
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            // Cleanup
            if (motors != null)
                motors.Dispose();

            if (led != null)
                led.Dispose();

            if (connection != null)
                connection.Dispose();
        }
    }
}
