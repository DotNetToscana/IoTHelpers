using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Gpio.Modules
{
    public class L298nMotorDriver : IDisposable
    {
        public Motor Motor1 { get; }    // Left Motor

        public Motor Motor2 { get; }    // Right Motor

        public L298nMotorDriver(int motor1Pin1, int motor1Pin2, int motor2Pin1, int motor2Pin2)
            : this(new Motor(motor1Pin1, motor1Pin2), new Motor(motor2Pin1, motor2Pin2))
        { }

        public L298nMotorDriver(Motor motor1, Motor motor2)
        {
            this.Motor1 = motor1;
            this.Motor2 = motor2;
        }

        public void Dispose()
        {
            Motor1?.Dispose();
            Motor2?.Dispose();
        }
    }

    public class Motor : IDisposable
    {
        private readonly GpioPin motorGpioPinA;
        private readonly GpioPin motorGpioPinB;

        public bool IsMoving { get; private set; }

        public MotorDirection Direction { get; private set; }

        public Motor(int pin1, int pin2)
        {
            var gpio = GpioController.GetDefault();

            motorGpioPinA = gpio.OpenPin(pin1);
            motorGpioPinB = gpio.OpenPin(pin2);
            motorGpioPinA.Write(GpioPinValue.Low);
            motorGpioPinB.Write(GpioPinValue.Low);
            motorGpioPinA.SetDriveMode(GpioPinDriveMode.Output);
            motorGpioPinB.SetDriveMode(GpioPinDriveMode.Output);
        }

        public void MoveForward()
        {
            IsMoving = true;
            Direction = MotorDirection.Forward;

            motorGpioPinA.Write(GpioPinValue.Low);
            motorGpioPinB.Write(GpioPinValue.High);
        }

        public Task MoveForwardAsync(TimeSpan interval)
            => this.MoveForwardAsync((int)interval.TotalMilliseconds);

        public async Task MoveForwardAsync(int milliseconds)
        {
            this.MoveForward();
            await this.WaitAndStopAsync(milliseconds);
        }

        public void MoveBackward()
        {
            IsMoving = true;
            Direction = MotorDirection.Backward;

            motorGpioPinA.Write(GpioPinValue.High);
            motorGpioPinB.Write(GpioPinValue.Low);
        }

        public Task MoveBackwardAsync(TimeSpan interval)
            => this.MoveBackwardAsync((int)interval.TotalMilliseconds);

        public async Task MoveBackwardAsync(int milliseconds)
        {
            this.MoveBackward();
            await this.WaitAndStopAsync(milliseconds);
        }

        public void Stop()
        {
            IsMoving = false;
            Direction = MotorDirection.None;

            motorGpioPinA.Write(GpioPinValue.Low);
            motorGpioPinB.Write(GpioPinValue.Low);
        }

        public void Dispose()
        {
            motorGpioPinA?.Dispose();
            motorGpioPinB?.Dispose();
        }

        private async Task WaitAndStopAsync(int milliseconds)
        {
            await Task.Delay(milliseconds);
            this.Stop();
        }
    }

    public enum MotorDirection
    {
        None,
        Forward,
        Backward
    }
}
