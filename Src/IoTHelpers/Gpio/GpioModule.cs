using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Gpio
{
    public enum LogicValue
    {
        Positive,
        Negative
    }

    public class GpioModule : GpioModuleBase
    {
        public GpioPinValue ActualLowPinValue { get; set; }

        public GpioPinValue ActualHighPinValue { get; set; }

        public GpioPin Pin { get; private set; }

        public GpioModule()
            : base(GpioController.GetDefault())
        { }

        public GpioModule(GpioController controller)
            : base(controller)
        { }

        public GpioModule(int pinNumber, GpioPinDriveMode driveMode = GpioPinDriveMode.Input, LogicValue logicValue = LogicValue.Positive)
            : this(GpioController.GetDefault(), pinNumber, driveMode, logicValue)
        { }

        public GpioModule(GpioController controller, int pinNumber, GpioPinDriveMode driveMode = GpioPinDriveMode.Input, LogicValue logicValue = LogicValue.Positive)
            : base(controller)
        {
            Pin = Controller.OpenPin(pinNumber);

            // Shows an error if the pin wasn't initialized properly
            if (Pin == null)
                throw new ArgumentException($"There were problems initializing the GPIO {GetType().Name} pin.");

            if (logicValue == LogicValue.Positive)
            {
                ActualHighPinValue = GpioPinValue.High;
                ActualLowPinValue = GpioPinValue.Low;
            }
            else
            {
                ActualHighPinValue = GpioPinValue.Low;
                ActualLowPinValue = GpioPinValue.High;
            }

            Pin.Write(ActualLowPinValue);
            Pin.SetDriveMode(driveMode);
        }

        public override void Dispose()
        {
            if (Pin != null)
            {
                Pin.Dispose();
                Pin = null;
            }

            base.Dispose();
        }
    }
}
