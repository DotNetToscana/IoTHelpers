using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;

namespace IoTHelpers.Modules
{
    public class ObstacleAdvoidanceSensor : GpioModule
    {
        private readonly DispatcherTimer timer;

        private readonly GpioPinValue noObstaclePinValue;
        private readonly GpioPinValue obstacleDetectedPinValue;

        private GpioPinValue lastPinValue;

        public bool HasObstacle { get; private set; } = false;

        public event EventHandler ObstacleDetected;
        public event EventHandler ObstacleRemoved;

        public ObstacleAdvoidanceSensor(int pinNumber) : base(pinNumber)
        {
            noObstaclePinValue = GpioPinValue.High;
            obstacleDetectedPinValue = GpioPinValue.Low;
            lastPinValue = noObstaclePinValue;

            Pin.SetDriveMode(GpioPinDriveMode.Input);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += CheckStatus;
            timer.Start();
        }

        private void CheckStatus(object sender, object e)
        {
            var currentPinValue = Pin.Read();

            // If same value of last read, exits. 
            if (currentPinValue == lastPinValue)
                return;

            // Checks the pin value.
            if (currentPinValue == obstacleDetectedPinValue)
            {
                HasObstacle = true;
                ObstacleDetected?.Invoke(this, EventArgs.Empty);
            }
            else if (currentPinValue == noObstaclePinValue)
            {
                HasObstacle = false;
                ObstacleRemoved?.Invoke(this, EventArgs.Empty);
            }

            lastPinValue = currentPinValue;
        }

        public override void Dispose()
        {
            timer.Stop();
            timer.Tick -= CheckStatus;

            base.Dispose();
        }
    }
}
