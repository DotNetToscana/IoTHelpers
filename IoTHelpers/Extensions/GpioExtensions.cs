using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Extensions
{
    internal static class GpioExtensions
    {
        public static void ChangeStatus(this GpioPin pin, GpioPinValue value) => pin.Write(value);

        public static bool IsHigh(this GpioPin pin) => pin.Read() == GpioPinValue.High;

        public static bool IsLow(this GpioPin pin) => pin.Read() == GpioPinValue.Low;
    }
}
