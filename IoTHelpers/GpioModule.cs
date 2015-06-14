using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers
{
    public abstract class GpioModule : IDisposable
    {
        protected  GpioController Controller { get; }

        public GpioPin Pin { get; }

        public GpioModule()
            : this(GpioController.GetDefault())
        { }

        protected GpioModule(GpioController controller)
        {
            Controller = controller;

            // Shows an error if there is no GPIO controller
            if (Controller == null)
                throw new ArgumentException("No GPIO controller found");
        }

        protected GpioModule(int pinNumber)
            : this(GpioController.GetDefault(), pinNumber)
        { }

        protected GpioModule(GpioController controller, int pinNumber)
            : this(controller)
        {
            Pin = Controller.OpenPin(pinNumber);

            // Shows an error if the pin wasn't initialized properly
            if (Pin == null)
                throw new ArgumentException($"There were problems initializing the GPIO {GetType().Name} pin.");
        }

        public virtual void Dispose()
        { }
    }
}
