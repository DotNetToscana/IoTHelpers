using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Gpio
{
    public abstract class GpioTimedModuleBase : TimedModule
    {
        protected GpioController Controller { get; }

        public GpioTimedModuleBase(ReadingMode mode, TimeSpan readInterval)
            : this(GpioController.GetDefault(), mode, readInterval)
        { }

        protected GpioTimedModuleBase(GpioController controller, ReadingMode mode, TimeSpan readInterval)
            : base(mode, readInterval)
        {
            Controller = controller;

            // Shows an error if there is no GPIO controller
            if (Controller == null)
                throw new ArgumentException("No GPIO controller found.");
        }
    }
}
