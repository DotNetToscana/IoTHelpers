using IoTHelpers.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Gpio.Modules
{
    public enum RotaryEncoderDirection
    {
        Clockwise,
        Anticlockwise
    }

    public class RotaryEncoder : GpioModuleBase
    {
        private readonly GpioModule dataPin;
        private readonly GpioModule clockPin;

        private int previousHighCount;

        public RotaryEncoderDirection direction;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public event EventHandler<RotaryEncoderEventArgs> Rotate;

        public RotaryEncoder(int dataPinNumber, int clockPinNumber, LogicValue logicValue = LogicValue.Positive)
        {
            dataPin = new GpioModule(Controller, dataPinNumber, GpioPinDriveMode.Input, logicValue);
            clockPin = new GpioModule(Controller, clockPinNumber, GpioPinDriveMode.Input, logicValue);

            dataPin.Pin.ValueChanged += Pin_ValueChanged;
            clockPin.Pin.ValueChanged += Pin_ValueChanged;
        }

        private void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            var dataValue = dataPin.Pin.Read();
            var clockValue = clockPin.Pin.Read();

            var highCount = (dataValue == GpioPinValue.High ? 1 : 0) + (clockValue == GpioPinValue.High ? 1 : 0);

            if (highCount == (previousHighCount + 1))
            {
                if (previousHighCount == 0)
                    direction = (dataValue == GpioPinValue.High) ? RotaryEncoderDirection.Anticlockwise : RotaryEncoderDirection.Clockwise;

                previousHighCount = highCount;
            }
            else if (previousHighCount == 2 && highCount == 1)
            {
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, Rotate, new RotaryEncoderEventArgs(direction), RaiseEventsOnUIThread);
                previousHighCount = 0;
            }
            else
            {
                previousHighCount = 0;
            }
        }        

        public override void Dispose()
        {
            if (dataPin != null)
            {
                dataPin.Pin.ValueChanged -= Pin_ValueChanged; 
                dataPin.Dispose();
            }

            if (clockPin != null)
            {
                clockPin.Pin.ValueChanged -= Pin_ValueChanged;
                clockPin.Dispose();
            }

            base.Dispose();
        }
    }

    public class RotaryEncoderEventArgs : EventArgs
    {
        public RotaryEncoderDirection Direction { get; }

        public RotaryEncoderEventArgs(RotaryEncoderDirection direction)
        {
            Direction = direction;
        }
    }
}

