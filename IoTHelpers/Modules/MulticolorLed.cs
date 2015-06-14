using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using IoTHelpers.Extensions;

namespace IoTHelpers.Modules
{
    public class MulticolorLed : GpioModule
    {
        private readonly SwitchGpioModule redPin;
        private readonly SwitchGpioModule bluePin;
        private readonly SwitchGpioModule greenPin;

        private const int REDLED_PIN = 22;
        private const int GREENLED_PIN = 18;
        private const int BLUELED_PIN = 27;

        public MulticolorLed(int redPinNumber,  int greenPinNumber, int bluePinNumber)
        {
            redPin = new SwitchGpioModule(Controller, redPinNumber);
            greenPin = new SwitchGpioModule(Controller, greenPinNumber);
            bluePin = new SwitchGpioModule(Controller, bluePinNumber);
        }

        public bool Red
        {
            get { return redPin.IsOn; }
            set { redPin.IsOn = value; }
        }

        public bool Green
        {
            get { return greenPin.IsOn; }
            set { greenPin.IsOn = value; }
        }

        public bool Blue
        {
            get { return bluePin.IsOn; }
            set { bluePin.IsOn = value; }
        }

        public void TurnOff()
        {
            redPin.TurnOff();
            greenPin.TurnOff();
            bluePin.TurnOff();
        }

        public void TurnWhite()
        {
            redPin.TurnOn();
            greenPin.TurnOn();
            bluePin.TurnOn();
        }

        public void TurnRed()
        {
            redPin.TurnOn();
            greenPin.TurnOff();
            bluePin.TurnOff();
        }

        public void TurnGreen()
        {
            redPin.TurnOff();
            greenPin.TurnOn();
            bluePin.TurnOff();
        }

        public void TurnBlue()
        {
            redPin.TurnOff();
            greenPin.TurnOff();
            bluePin.TurnOn();
        }

        public override void Dispose()
        {
            if (redPin != null)
                redPin.Dispose();

            if (greenPin != null)
                greenPin.Dispose();

            if (bluePin != null)
                bluePin.Dispose();
        }
    }
}

