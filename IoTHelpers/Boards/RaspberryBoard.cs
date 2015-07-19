using IoTHelpers.Gpio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Boards
{
	public class RaspberryBoard : Board
	{
		private static readonly Lazy<RaspberryBoard> board = new Lazy<RaspberryBoard>(() =>
		{
			try
			{
				var board = new RaspberryBoard();
				return board;
			}
			catch
			{
				return null;
			}
		});

		public static RaspberryBoard GetDefault() => board.Value;

		public SwitchGpioModule PowerLed { get; private set; }

		public SwitchGpioModule StatusLed { get; private set; }

		public const int PowerLedNumber = 35;
		public const int GreenLedNumber = 47;

		protected RaspberryBoard()
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
