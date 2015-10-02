using IoTHelpers.Gpio.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTHelpers.Gpio.Extensions
{
    public static class L298nMotorDriverExtensions
    {
        public static LeftRightMotors AsLeftRightMotors(this L298nMotorDriver motorDriver) 
            => new LeftRightMotors(motorDriver);
    }

    public class LeftRightMotors : L298nMotorDriver
    {
        public bool IsMoving => Motor1.IsMoving || Motor2.IsMoving;

        internal LeftRightMotors(L298nMotorDriver motorDriver) 
            : base(motorDriver.Motor1, motorDriver.Motor2)
        { }

        public void MoveForward()
        {
            Motor1.MoveForward();
            Motor2.MoveForward();
        }

        public async Task MoveForwardAsync(int milliseconds)
        {
            this.MoveForward();
            await this.WaitAndStopAsync(milliseconds);
        }

        public void MoveBackward()
        {
            Motor1.MoveBackward();
            Motor2.MoveBackward();
        }

        public async Task MoveBackwardAsync(int milliseconds)
        {
            this.MoveBackward();
            await this.WaitAndStopAsync(milliseconds);
        }

        public void TurnRight()
        {
            Motor2.Stop();
            Motor1.MoveForward();
        }

        public async Task TurnRightAsync(int milliseconds)
        {
            this.TurnRight();
            await this.WaitAndStopAsync(milliseconds);
        }

        public void TurnLeft()
        {
            Motor1.Stop();
            Motor2.MoveForward();
        }

        public async Task TurnLeftAsync(int milliseconds)
        {
            this.TurnLeft();
            await this.WaitAndStopAsync(milliseconds);
        }

        public void RotateRight()
        {
            Motor1.MoveForward();
            Motor2.MoveBackward();
        }

        public async Task RotateRightAsync(int milliseconds)
        {
            this.RotateRight();
            await this.WaitAndStopAsync(milliseconds);
        }

        public void RotateLeft()
        {
            Motor1.MoveBackward();
            Motor2.MoveForward();
        }

        public async Task RotateLeftAsync(int milliseconds)
        {
            this.RotateLeft();
            await this.WaitAndStopAsync(milliseconds);
        }

        public void Stop()
        {
            Motor1.Stop();
            Motor2.Stop();
        }

        private async Task WaitAndStopAsync(int milliseconds)
        {
            await Task.Delay(milliseconds);
            this.Stop();
        }
    }
}
