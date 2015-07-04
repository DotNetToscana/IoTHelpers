using IoTHelpers.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace IoTHelpers.I2c.Devices
{
    public struct Acceleration
    {
        public double X { get; internal set; }

        public double Y { get; internal set; }

        public double Z { get; internal set; }
    };

    public class Adxl345Accelerometer : I2cDeviceBase
    {
        // TSL Address Constants 
        public const byte ACCEL_I2C_ADDR = 0x53;            /* 7-bit I2C address of the ADXL345 with SDO pulled low */

        private const byte ACCEL_REG_POWER_CONTROL = 0x2D;  /* Address of the Power Control register */
        private const byte ACCEL_REG_DATA_FORMAT = 0x31;    /* Address of the Data Format register   */
        private const byte ACCEL_REG_X = 0x32;              /* Address of the X Axis data register   */
        private const byte ACCEL_REG_Y = 0x34;              /* Address of the Y Axis data register   */
        private const byte ACCEL_REG_Z = 0x36;              /* Address of the Z Axis data register   */

        private const int ACCEL_RES = 1024;                             /* The ADXL345 has 10 bit resolution giving 1024 unique values */
        private const int ACCEL_DYN_RANGE_G = 8;                        /* The ADXL345 had a total dynamic range of 8G, since we're configuring it to +-4G */
        private const int UNITS_PER_G = ACCEL_RES / ACCEL_DYN_RANGE_G;  /* Ratio of raw int values to G units */

        private Timer timer;

        private Acceleration lastAcceleration = new Acceleration();

        public Acceleration CurrentAcceleration { get; private set; } = new Acceleration();

        public bool RaiseEventsOnUIThread { get; set; } = false;

        private TimeSpan readInterval = TimeSpan.FromMilliseconds(100);
        public TimeSpan ReadInterval
        {
            get { return readInterval; }
            set
            {
                readInterval = value;
                if (timer != null)
                    timer.Change((int)readInterval.TotalMilliseconds, (int)readInterval.TotalMilliseconds);
            }
        }

        public event EventHandler AccelerationChanged;

        public Adxl345Accelerometer(int slaveAddress = ACCEL_I2C_ADDR, I2cBusSpeed busSpeed = I2cBusSpeed.FastMode, I2cSharingMode sharingMode = I2cSharingMode.Shared, string i2cControllerName = I2C_CONTROLLER_NAME)
            : base(slaveAddress, busSpeed, sharingMode, i2cControllerName)
        { }

        protected override Task InitializeSensorAsync()
        {
            /* 
            * Initialize the accelerometer:
            *
            * For this device, we create 2-byte write buffers:
            * The first byte is the register address we want to write to.
            * The second byte is the contents that we want to write to the register. 
            */
            var writeBuf_DataFormat = new byte[] { ACCEL_REG_DATA_FORMAT, 0x01 };        /* 0x01 sets range to +- 4Gs                         */
            var writeBuf_PowerControl = new byte[] { ACCEL_REG_POWER_CONTROL, 0x08 };    /* 0x08 puts the accelerometer into measurement mode */

            /* Write the register settings */
            Device.Write(writeBuf_DataFormat);
            Device.Write(writeBuf_PowerControl);

            /* Now that everything is initialized, create a timer so we read data every 100mS */
            timer = new Timer(ReadSensor, null, 0, (int)readInterval.TotalMilliseconds);

            return Task.FromResult<object>(null);
        }

        private void ReadSensor(object state)
        {
            CurrentAcceleration = this.ReadAcceleration();

            if (CurrentAcceleration.X != lastAcceleration.X || CurrentAcceleration.Y != lastAcceleration.Y || CurrentAcceleration.Z != lastAcceleration.Z)
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, AccelerationChanged, RaiseEventsOnUIThread);

            lastAcceleration = CurrentAcceleration;
        }

        private Acceleration ReadAcceleration()
        {
            var regAddrBuf = new byte[] { ACCEL_REG_X };  /* Register address we want to read from                                         */
            var readBuf = new byte[6];                    /* We read 6 bytes sequentially to get all 3 two-byte axes registers in one read */

            /* 
             * Read from the accelerometer 
             * We call WriteRead() so we first write the address of the X-Axis I2C register, then read all 3 axes
             */
            Device.WriteRead(regAddrBuf, readBuf);

            /* 
             * In order to get the raw 16-bit data values, we need to concatenate two 8-bit bytes from the I2C read for each axis.
             * We accomplish this by using the BitConverter class.
             */
            var accelerationRawX = BitConverter.ToInt16(readBuf, 0);
            var accelerationRawY = BitConverter.ToInt16(readBuf, 2);
            var accelerationRawZ = BitConverter.ToInt16(readBuf, 4);

            /* Convert raw values to G's */
            var accel = new Acceleration
            {
                X = (double)accelerationRawX / UNITS_PER_G,
                Y = (double)accelerationRawY / UNITS_PER_G,
                Z = (double)accelerationRawZ / UNITS_PER_G
            };

            return accel;
        }

        public override void Dispose()
        {
            if (timer != null)
                timer.Dispose();

            base.Dispose();
        }
    }
}
