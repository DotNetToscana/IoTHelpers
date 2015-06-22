using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Gpio
{
    public class SwitchGpioModule : GpioModule
    {
        public SwitchGpioModule(int pinNumber, LogicValue logicValue = LogicValue.Positive)
            : this(GpioController.GetDefault(), pinNumber, logicValue)
        { }

        public SwitchGpioModule(GpioController controller, int pinNumber, LogicValue logicValue = LogicValue.Positive)
            : base(controller, pinNumber, GpioPinDriveMode.Output, logicValue)
        {
            this.TurnOff();
        }


        public virtual bool IsOn
        {
            get { return Pin.Read() ==  ActualHighPinValue; }
            set { Pin.Write(value ? ActualHighPinValue : ActualLowPinValue); }
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
            base.Dispose();
        }
    }
}

