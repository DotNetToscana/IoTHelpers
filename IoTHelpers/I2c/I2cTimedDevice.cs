using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace IoTHelpers.I2c
{
    public abstract class I2cTimedDevice : I2cDeviceBase
    {
        private readonly Timer timer;

        private ReadingMode mode;
        public ReadingMode Mode
        {
            get { return mode; }
            set { this.Initialize(value); }
        }

        private TimeSpan readInterval;
        public TimeSpan ReadInterval
        {
            get { return readInterval; }
            set
            {
                if (Mode == ReadingMode.Manual)
                    throw new NotSupportedException($"You cannot change {nameof(ReadInterval)} when {nameof(Mode)} is set to {ReadingMode.Manual}.");

                readInterval = value;
                if (timer != null)
                    timer.Change(0, (int)readInterval.TotalMilliseconds);
            }
        }

        public I2cTimedDevice(int slaveAddress, ReadingMode mode, TimeSpan readInterval,
            I2cBusSpeed busSpeed = I2cBusSpeed.FastMode, I2cSharingMode sharingMode = I2cSharingMode.Shared, string i2cControllerName = RaspberryPiI2cControllerName)
            : base(slaveAddress, busSpeed, sharingMode, i2cControllerName)
        {
            this.readInterval = readInterval;
            this.mode = mode;

            timer = new Timer(CheckState, null, Timeout.Infinite, Timeout.Infinite);
        }
        
        protected void InitializeTimer() => this.Initialize(mode);

        private void Initialize(ReadingMode newMode)
        {
            this.mode = newMode;

            if (mode == ReadingMode.Continuous)
                timer.Change(0, (int)readInterval.TotalMilliseconds);
            else
                timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void CheckState(object state) => OnTimer();

        protected abstract void OnTimer();

        public virtual new void Dispose()
        {
            timer.Dispose();
            base.Dispose();
        }
    }
}
