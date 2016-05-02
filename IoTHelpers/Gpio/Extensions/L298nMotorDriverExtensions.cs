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

        public LeftRightMotorsDirection Direction
        {
            get
            {
                if (Motor1.Direction == MotorDirection.Forward && Motor2.Direction == MotorDirection.Forward)
                    return LeftRightMotorsDirection.Forward;

                if (Motor1.Direction == MotorDirection.Backward && Motor2.Direction == MotorDirection.Backward)
                    return LeftRightMotorsDirection.Backward;

                if (Motor1.Direction == MotorDirection.None && Motor2.Direction == MotorDirection.Forward)
                    return LeftRightMotorsDirection.ForwardLeft;

                if (Motor1.Direction == MotorDirection.Forward && Motor2.Direction == MotorDirection.None)
                    return LeftRightMotorsDirection.ForwardRight;

                if (Motor1.Direction == MotorDirection.None && Motor2.Direction == MotorDirection.Backward)
                    return LeftRightMotorsDirection.BackwardLeft;

                if (Motor1.Direction == MotorDirection.Backward && Motor2.Direction == MotorDirection.None)
                    return LeftRightMotorsDirection.BackwardRight;

                if (Motor1.Direction == MotorDirection.Forward && Motor2.Direction == MotorDirection.Backward)
                    return LeftRightMotorsDirection.RotateRight;

                if (Motor1.Direction == MotorDirection.Backward && Motor2.Direction == MotorDirection.Forward)
                    return LeftRightMotorsDirection.RotateLeft;

                return LeftRightMotorsDirection.None;
            }
        }

        internal LeftRightMotors(L298nMotorDriver motorDriver)
            : base(motorDriver.Motor1, motorDriver.Motor2)
        { }

        public void MoveForward()
        {
            Motor1.MoveForward();
            Motor2.MoveForward();
        }

        public Task MoveForwardAsync(TimeSpan interval)
            => this.MoveForwardAsync((int)interval.TotalMilliseconds);

        public async Task MoveForwardAsync(int milliseconds)
        {
            this.MoveForward();
            await this.WaitAndStopAsync(milliseconds);
        }

        public void MoveForwardLeft()
        {
            Motor1.Stop();
            Motor2.MoveForward();
        }

        public Task MoveForwardLeftAsync(TimeSpan interval)
            => this.MoveForwardLeftAsync((int)interval.TotalMilliseconds);

        public async Task MoveForwardLeftAsync(int milliseconds)
        {
            this.MoveForwardLeft();
            await this.WaitAndStopAsync(milliseconds);
        }

        public void MoveForwardRight()
        {
            Motor2.Stop();
            Motor1.MoveForward();
        }

        public Task MoveForwardRightAsync(TimeSpan interval)
            => this.MoveForwardRightAsync((int)interval.TotalMilliseconds);

        public async Task MoveForwardRightAsync(int milliseconds)
        {
            this.MoveForwardRight();
            await this.WaitAndStopAsync(milliseconds);
        }

        public void MoveBackward()
        {
            Motor1.MoveBackward();
            Motor2.MoveBackward();
        }

        public Task MoveBackwardAsync(TimeSpan interval)
            => this.MoveBackwardAsync((int)interval.TotalMilliseconds);

        public async Task MoveBackwardAsync(int milliseconds)
        {
            this.MoveBackward();
            await this.WaitAndStopAsync(milliseconds);
        }


        public void MoveBackwardLeft()
        {
            Motor1.Stop();
            Motor2.MoveBackward();
        }

        public Task MoveBackwardLeftAsync(TimeSpan interval)
            => this.MoveBackwardLeftAsync((int)interval.TotalMilliseconds);

        public async Task MoveBackwardLeftAsync(int milliseconds)
        {
            this.MoveBackwardLeft();
            await this.WaitAndStopAsync(milliseconds);
        }

        public void MoveBackwardRight()
        {
            Motor2.Stop();
            Motor1.MoveBackward();
        }

        public Task MoveBackwardRightAsync(TimeSpan interval)
            => this.MoveBackwardRightAsync((int)interval.TotalMilliseconds);

        public async Task MoveBackwardRightAsync(int milliseconds)
        {
            this.MoveBackwardRight();
            await this.WaitAndStopAsync(milliseconds);
        }

        public void RotateLeft()
        {
            Motor1.MoveBackward();
            Motor2.MoveForward();
        }

        public Task RotateLeftAsync(TimeSpan interval)
            => this.RotateLeftAsync((int)interval.TotalMilliseconds);

        public async Task RotateLeftAsync(int milliseconds)
        {
            this.RotateLeft();
            await this.WaitAndStopAsync(milliseconds);
        }

        public void RotateRight()
        {
            Motor1.MoveForward();
            Motor2.MoveBackward();
        }

        public Task RotateRightAsync(TimeSpan interval)
            => this.RotateRightAsync((int)interval.TotalMilliseconds);

        public async Task RotateRightAsync(int milliseconds)
        {
            this.RotateRight();
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

    public enum LeftRightMotorsDirection
    {
        None,
        Forward,
        ForwardLeft,
        ForwardRight,
        Backward,
        BackwardLeft,
        BackwardRight,
        RotateLeft,
        RotateRight
    }
}
