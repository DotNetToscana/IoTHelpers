using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Gpio
{
    internal static class GpioExtensions
    {
        public static bool IsHigh(this GpioPin pin) => pin.Read() == GpioPinValue.High;

        public static bool IsLow(this GpioPin pin) => pin.Read() == GpioPinValue.Low;
    }
}
