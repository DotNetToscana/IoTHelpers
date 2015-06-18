using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Gpio
{
    public abstract class GpioModule : GpioModuleBase
    {
        public GpioPin Pin { get; }

        protected GpioModule()
            : base(GpioController.GetDefault())
        { }

        protected GpioModule(GpioController controller) 
            : base(controller)
        { }        

        protected GpioModule(int pinNumber)
            : this(GpioController.GetDefault(), pinNumber)
        { }

        protected GpioModule(GpioController controller, int pinNumber)
            : base(controller)
        {
            Pin = Controller.OpenPin(pinNumber);

            // Shows an error if the pin wasn't initialized properly
            if (Pin == null)
                throw new ArgumentException($"There were problems initializing the GPIO {GetType().Name} pin.");
        }

        public override void Dispose()
        {
            if (Pin != null)
                Pin.Dispose();

            base.Dispose();
        }
    }
}
