using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Gpio.Modules
{
    public class InfraredTransmitter : SwitchGpioModule
    {
        public InfraredTransmitter(int pinNumber, LogicValue logicValue = LogicValue.Positive)
            : base(pinNumber, logicValue)
        { }   
    }
}

