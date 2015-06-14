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
        protected GpioController Controller { get; }

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

        public virtual void Dispose()
        { }
    }
}
