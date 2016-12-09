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
    public class Tsl2561LuminositySensor : I2cTimedDevice
    {
        // TSL Address Constants 
        public const int DefaultI2cAddress = 0x39;      // default address  
        public const int TSL2561_ADDR_0 = 0x29;         // address with '0' shorted on board  
        public const int TSL2561_ADDR_1 = 0x49;         // address with '1' shorted on board  

        // TSL Commands 
        private const int TSL2561_CMD = 0x80;
        private const int TSL2561_CMD_CLEAR = 0xC0;

        // TSL Registers 
        private const int TSL2561_REG_CONTROL = 0x00;
        private const int TSL2561_REG_TIMING = 0x01;
        private const int TSL2561_REG_THRESH_L = 0x02;
        private const int TSL2561_REG_THRESH_H = 0x04;
        private const int TSL2561_REG_INTCTL = 0x06;
        private const int TSL2561_REG_ID = 0x0A;
        private const int TSL2561_REG_DATA_0 = 0x0C;
        private const int TSL2561_REG_DATA_1 = 0x0E;

        private static TimeSpan DEFAULT_READ_INTERVAL = TimeSpan.FromSeconds(3);

        // TSL Gain and MS Values
        private bool gain = false;
        private uint ms = 0;

        private double currentLux;
        public double CurrentLux
        {
            get
            {
                if (ReadingMode == ReadingMode.Manual)
                    throw new NotSupportedException($"{nameof(CurrentLux)} is available only when {nameof(ReadingMode)} is set to {ReadingMode.Continuous}.");

                return currentLux;
            }
        }

        public double CurrentLightLevel
        {
            get
            {
                if (ReadingMode == ReadingMode.Manual)
                    throw new NotSupportedException($"{nameof(CurrentLightLevel)} is available only when {nameof(ReadingMode)} is set to {ReadingMode.Continuous}.");

                return this.ConvertToLightLevel(currentLux);
            }
        }

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public event EventHandler LuxChanged;

        public Tsl2561LuminositySensor(int slaveAddress = DefaultI2cAddress, ReadingMode mode = ReadingMode.Continuous, I2cBusSpeed busSpeed = I2cBusSpeed.FastMode, I2cSharingMode sharingMode = I2cSharingMode.Shared, string i2cControllerName = RaspberryPiI2cControllerName)
            : base(slaveAddress, mode, DEFAULT_READ_INTERVAL, busSpeed, sharingMode, i2cControllerName)
        { }

        protected override Task InitializeSensorAsync()
        {
            // Set the TSL Timing
            ms = (uint)this.SetTiming(false, 2);

            // Powerup the TSL sensor
            this.PowerUp();

            base.InitializeTimer();
            return Task.FromResult<object>(null);
        }

        protected override void OnTimer()
        {
            // Retrive luminosity value.
            var lux = this.GetLux();

            if (currentLux != lux)
            {
                currentLux = lux;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, LuxChanged, RaiseEventsOnUIThread);
            }
        }

        // TSL2561 Sensor Power up
        private void PowerUp() => Write8(TSL2561_REG_CONTROL, 0x03);

        // TSL2561 Sensor Power down
        private void PowerDown() => Write8(TSL2561_REG_CONTROL, 0x00);

        // Retrieve TSL ID
        private byte GetId() => this.Read8(TSL2561_REG_ID);

        // Set TSL2561 Timing and return the MS
        private int SetTiming(bool gain, byte time)
        {
            var ms = 0;

            switch (time)
            {
                case 0: ms = 14; break;
                case 1: ms = 101; break;
                case 2: ms = 402; break;
                default: ms = 0; break;
            }

            int timing = this.Read8(TSL2561_REG_TIMING);

            // Set gain (0 or 1) 
            if (gain)
                timing |= 0x10;
            else
                timing &= (~0x10);

            // Set integration time (0 to 3)
            timing &= ~0x03;
            timing |= (time & 0x03);

            this.Write8(TSL2561_REG_TIMING, (byte)timing);

            return ms;
        }

        // Get channel data
        private uint[] GetData()
        {
            var data = new uint[2];
            data[0] = Read16(TSL2561_REG_DATA_0);
            data[1] = Read16(TSL2561_REG_DATA_1);

            return data;
        }

        // Calculate Lux
        private double GetLux(bool gain, uint ms, uint ch0, uint ch1)
        {
            double ratio, d0, d1;
            double lux = 0.0;

            // Determine if either sensor saturated (0xFFFF)
            // If so, abandon ship (calculation will not be accurate)
            if ((ch0 == 0xFFFF) || (ch1 == 0xFFFF))
            {
                lux = 0.0;
                return lux;
            }

            // Convert from unsigned integer to floating point
            d0 = ch0; d1 = ch1;

            // We will need the ratio for subsequent calculations
            ratio = d1 / d0;

            // Normalize for integration time
            d0 *= (402.0 / ms);
            d1 *= (402.0 / ms);

            // Normalize for gain
            if (!gain)
            {
                d0 *= 16;
                d1 *= 16;
            }

            // Determine lux per datasheet equations:
            if (ratio < 0.5)
                lux = 0.0304 * d0 - 0.062 * d0 * Math.Pow(ratio, 1.4);
            else if (ratio < 0.61)
                lux = 0.0224 * d0 - 0.031 * d1;
            else if (ratio < 0.80)
                lux = 0.0128 * d0 - 0.0153 * d1;
            else if (ratio < 1.30)
                lux = 0.00146 * d0 - 0.00112 * d1;
            else
                lux = 0.0;

            return lux;
        }

        public double GetLux()
        {
            var data = this.GetData();
            var lux = this.GetLux(gain, ms, data[0], data[1]);

            return lux;
        }

        public double GetLightLevel()
        {
            var lux = this.GetLux();
            return this.ConvertToLightLevel(lux);
        }

        // Write byte
        private void Write8(byte addr, byte cmd)
        {
            var command = new byte[] { (byte)((addr) | TSL2561_CMD), cmd };
            Device.Write(command);
        }

        // Read byte
        private byte Read8(byte addr)
        {
            var aaddr = new byte[] { (byte)((addr) | TSL2561_CMD) };
            var data = new byte[1];

            Device.WriteRead(aaddr, data);
            return data[0];
        }

        // Read integer
        private ushort Read16(byte addr)
        {
            var aaddr = new byte[] { (byte)((addr) | TSL2561_CMD) };
            var data = new byte[2];

            Device.WriteRead(aaddr, data);
            return (ushort)((data[1] << 8) | (data[0]));
        }

        private double ConvertToLightLevel(double lux)
        {
            // https://msdn.microsoft.com/en-us/library/windows/desktop/dd319008(v=vs.85).aspx
            var lightLevel = Math.Abs(Math.Log10(currentLux) / 5);
            if (lightLevel == double.PositiveInfinity)
                lightLevel = 0;

            return lightLevel;
        }

        public override void Dispose()
        {
            // Powerdown the TSL sensor
            this.PowerDown();

            base.Dispose();
        }
    }
}
