using IoTHelpers.Gpio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Boards
{
    public class RaspberryPi2Board : Board
    {
        private static readonly Lazy<RaspberryPi2Board> board = new Lazy<RaspberryPi2Board>(() =>
        {
            try
            {
                if (DeviceInformation.Type == DeviceType.RaspberryPi2)
                {
                    var board = new RaspberryPi2Board();
                    return board;
                }

                return null;
            }
            catch
            {
                return null;
            }
        });

        public static RaspberryPi2Board GetDefault() => board.Value;

        public SwitchGpioModule PowerLed { get; private set; }

        public SwitchGpioModule StatusLed { get; private set; }

        public const int PowerLedNumber = 35;
        public const int GreenLedNumber = 47;

        protected RaspberryPi2Board()
        {
            var controller = GpioController.GetDefault();

            PowerLed = new SwitchGpioModule(controller, PowerLedNumber);
            StatusLed = new SwitchGpioModule(controller, GreenLedNumber);

            PowerLed.TurnOn();
        }

        public override void Dispose()
        {
            if (PowerLed != null)
            {
                PowerLed.Dispose();
                PowerLed = null;
            }

            if (StatusLed != null)
            {
                StatusLed.Dispose();
                StatusLed = null;
            }

            base.Dispose();
        }
    }
}
