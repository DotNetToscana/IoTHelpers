using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Gpio.Modules
{
    public class Relay : SwitchGpioModule
    {
        public Relay(int pinNumber, GpioPinValue onValue = GpioPinValue.High)
            : base(pinNumber, onValue)
        { }   
    }
}

