using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Gpio.Modules
{
    public class TwoColorLed : GpioModuleBase
    {
        private readonly SwitchGpioModule color1Pin;
        private readonly SwitchGpioModule color2Pin;

        public TwoColorLed(int color1PinNumber, int color2PinNumber, LogicValue logicValue = LogicValue.Positive)
        {
            color1Pin = new SwitchGpioModule(Controller, color1PinNumber, logicValue);
            color2Pin = new SwitchGpioModule(Controller, color2PinNumber, logicValue);
        }

        public bool Color1
        {
            get { return color1Pin.IsOn; }
            set { color1Pin.IsOn = value; }
        }

        public bool Color2
        {
            get { return color2Pin.IsOn; }
            set { color2Pin.IsOn = value; }
        }



        public void TurnOff()
        {
            color2Pin.TurnOff();
            color1Pin.TurnOff();
        }

        public void TurnColor2()
        {
            color1Pin.TurnOff();
            color2Pin.TurnOn();
        }

        public void TurnColor1()
        {
            color1Pin.TurnOn();
            color2Pin.TurnOff();
        }

        public override void Dispose()
        {
            if (color1Pin != null)
                color1Pin.Dispose();

            if (color2Pin != null)
                color2Pin.Dispose();

            base.Dispose();
        }
    }
}

