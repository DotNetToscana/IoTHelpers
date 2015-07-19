using IoTHelpers.Gpio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Boards
{
    public class RaspberryPiBoard : Board
    {
        private static readonly Lazy<RaspberryPiBoard> board = new Lazy<RaspberryPiBoard>(() =>
        {
            try
            {
                var board = new RaspberryPiBoard();
                return board;
            }
            catch
            {
                return null;
            }
        });

        public static RaspberryPiBoard GetDefault() => board.Value;

        public SwitchGpioModule PowerLed { get; private set; }

        public SwitchGpioModule StatusLed { get; private set; }

        public const int PowerLedNumber = 35;
        public const int GreenLedNumber = 47;

        protected RaspberryPiBoard()
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
