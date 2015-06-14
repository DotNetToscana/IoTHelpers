using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using IoTHelpers.Extensions;

namespace IoTHelpers.Modules
{
    public class Laser : SwitchGpioModule
    {
        private const int LASER_PIN = 6;

        public Laser(int laserPinNumber, GpioPinValue onValue = GpioPinValue.High)
            : base(laserPinNumber, onValue)
        { }   
    }
}

