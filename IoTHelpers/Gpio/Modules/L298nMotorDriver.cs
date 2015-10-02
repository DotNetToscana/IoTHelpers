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
            if (Motor1 != null)
                Motor1.Dispose();

            if (Motor2 != null)
                Motor2.Dispose();
        }
    }

    public class Motor : IDisposable
    {
        private readonly GpioPin motorGpioPinA;
        private readonly GpioPin motorGpioPinB;

        public bool IsMoving { get; private set; }

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

            motorGpioPinA.Write(GpioPinValue.Low);
            motorGpioPinB.Write(GpioPinValue.High);
        }

        public async Task MoveForwardAsync(int milliseconds)
        {
            this.MoveForward();
            await this.WaitAndStopAsync(milliseconds);
        }

        public void MoveBackward()
        {
            IsMoving = true;

            motorGpioPinA.Write(GpioPinValue.High);
            motorGpioPinB.Write(GpioPinValue.Low);
        }

        public async Task MoveBackwardAsync(int milliseconds)
        {
            this.MoveBackward();
            await this.WaitAndStopAsync(milliseconds);
        }

        public void Stop()
        {
            IsMoving = false;

            motorGpioPinA.Write(GpioPinValue.Low);
            motorGpioPinB.Write(GpioPinValue.Low);
        }

        public void Dispose()
        {
            if (motorGpioPinA != null)
                motorGpioPinA.Dispose();

            if (motorGpioPinB != null)
                motorGpioPinB.Dispose();
        }

        private async Task WaitAndStopAsync(int milliseconds)
        {
            await Task.Delay(milliseconds);
            this.Stop();
        }
    }
}
