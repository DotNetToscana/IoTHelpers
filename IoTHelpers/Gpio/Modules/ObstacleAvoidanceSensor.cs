using IoTHelpers.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace IoTHelpers.Gpio.Modules
{
    public class ObstacleAdvoidanceSensor : GpioModule
    {
        private readonly Timer timer;

        private GpioPinValue lastPinValue;

        public bool HasObstacle { get; private set; } = false;

        public event EventHandler ObstacleDetected;
        public event EventHandler ObstacleRemoved;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public ObstacleAdvoidanceSensor(int pinNumber) : base(pinNumber, GpioPinDriveMode.Input, LogicValue.Negative)
        {
            lastPinValue = ActualLowPinValue;

            timer = new Timer(CheckState, null, 0, 100);
        }

        private void CheckState(object state)
        {
            var currentPinValue = Pin.Read();

            // If same value of last read, exits. 
            if (currentPinValue == lastPinValue)
                return;

            // Checks the pin value.
            if (currentPinValue == ActualHighPinValue)
            {
                HasObstacle = true;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, ObstacleDetected, RaiseEventsOnUIThread);
            }
            else if (currentPinValue == ActualLowPinValue)
            {
                HasObstacle = false;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, ObstacleRemoved, RaiseEventsOnUIThread);
            }

            lastPinValue = currentPinValue;
        }

        public override void Dispose()
        {
            timer.Dispose();
            base.Dispose();
        }
    }
}
