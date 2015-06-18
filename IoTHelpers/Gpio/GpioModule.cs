using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Gpio
{
    public class GpioModule : GpioModuleBase
    {
        public GpioPin Pin { get; }

        public GpioModule()
            : base(GpioController.GetDefault())
        { }

        public GpioModule(GpioController controller) 
            : base(controller)
        { }

        public GpioModule(int pinNumber, GpioPinDriveMode driveMode = GpioPinDriveMode.Input)
            : this(GpioController.GetDefault(), pinNumber, driveMode)
        { }

        public GpioModule(GpioController controller, int pinNumber, GpioPinDriveMode driveMode = GpioPinDriveMode.Input)
            : base(controller)
        {
            Pin = Controller.OpenPin(pinNumber);

            // Shows an error if the pin wasn't initialized properly
            if (Pin == null)
                throw new ArgumentException($"There were problems initializing the GPIO {GetType().Name} pin.");

            Pin.SetDriveMode(driveMode);
        }

        public override void Dispose()
        {
            if (Pin != null)
                Pin.Dispose();

            base.Dispose();
        }
    }
}
