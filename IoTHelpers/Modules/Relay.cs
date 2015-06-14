using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using IoTHelpers.Extensions;

namespace IoTHelpers.Modules
{
    public class Relay : SwitchGpioModule
    {
        private const int RELAY_PIN = 24;

        public Relay(int relayPinNumber, GpioPinValue onValue = GpioPinValue.High)
            : base(relayPinNumber, onValue)
        { }   
    }
}

