using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using IoTHelpers.Extensions;

namespace IoTHelpers.Modules
{
    public class SwitchGpioModule : GpioModule
    {
        public GpioPin Pin { get; }

        public GpioPinValue OnValue { get; }

        public GpioPinValue OffValue { get; }

        public SwitchGpioModule(int pinNumber, GpioPinValue onValue = GpioPinValue.High)
            : this(GpioController.GetDefault(), pinNumber, onValue)
        { }

        public SwitchGpioModule(GpioController controller, int pinNumber, GpioPinValue onValue = GpioPinValue.High)
            : base(controller)
        {
            Pin = Controller.OpenPin(pinNumber);

            // Shows an error if the pin wasn't initialized properly
            if (Pin == null)
                throw new ArgumentException($"There were problems initializing the GPIO {GetType().Name} pin.");

            if (onValue == GpioPinValue.High)
            {
                OnValue = GpioPinValue.High;
                OffValue = GpioPinValue.Low;
            }
            else
            {
                OnValue = GpioPinValue.Low;
                OffValue = GpioPinValue.High;
            }

            // Tests and initializes the pin.           
            Pin.SetDriveMode(GpioPinDriveMode.Output);
            this.TurnOff();
        }

        public virtual bool IsOn
        {
            get { return Pin.Read() == OnValue; }
            set { Pin.Write(value ? OnValue : OffValue); }
        }

        public virtual void TurnOn()
        {
            IsOn = true;
        }

        public virtual void TurnOff()
        {
            IsOn = false;
        }

        public virtual void Toogle()
        {
            IsOn = !IsOn;
        }

        public override void Dispose()
        {
            if (Pin != null)
                Pin.Dispose();
        }
    }
}

