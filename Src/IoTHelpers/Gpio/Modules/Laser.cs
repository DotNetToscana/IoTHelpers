using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Gpio.Modules
{
    public class Laser : SwitchGpioModule
    {
        public Laser(int pinNumber, LogicValue logicValue = LogicValue.Positive)
            : base(pinNumber, logicValue)
        { }   
    }
}

