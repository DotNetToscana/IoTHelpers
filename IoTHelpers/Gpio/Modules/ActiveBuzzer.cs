using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Gpio.Modules
{
    public class ActiveBuzzer : SwitchGpioModule
    {
        public ActiveBuzzer(int pinNumber, GpioPinValue onValue = GpioPinValue.High)
            : base(pinNumber, onValue)
        { }   
    }
}

