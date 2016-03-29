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
    public class Bmp180BarometricSensor : I2cTimedDevice
    {
        public enum Bmp180AccuracyMode
        {
            UltraLowPower = 0,
            Standard = 1,
            HighResolution = 2,
            UltraHighResolution = 3
        }

        // BMP180 Address Constants 
        public const int DefaultI2cAddress = 0x77;      // default address  

        /// <summary>
        /// 8-bit I2C read address of the BMP180. For testing communications only. Returns 0x55.
        /// </summary>
        private const byte BMP180_REG_CHIPID = 0xD0;

        /// <summary>
        /// 8-bit I2C command address of the BMP180
        /// </summary>
        private const byte BMP180_REG_CONTROL = 0xF4;

        /// <summary>
        /// 8-bit I2C read address of the BMP180
        /// </summary>
        private const byte BMP180_REG_RESULT = 0xF6;

        /// <summary>
        /// 8-bit I2C command for the temperature reading of the BMP180
        /// </summary>
        private const byte BMP180_COM_TEMPERATURE = 0x2E;

        /// <summary>
        /// 8-bit I2C command for the Ultra Low Power Mode Pressure reading of the BMP180
        /// </summary>
        private const byte BMP180_COM_PRESSURE0 = 0x34;

        /// <summary>
        /// 8-bit I2C command for the Standard Mode Pressure reading of the BMP180
        /// </summary>
        private const byte BMP180_COM_PRESSURE1 = 0x74;

        /// <summary>
        /// 8-bit I2C command for the High Resolution Mode Pressure reading of the BMP180
        /// </summary>
        private const byte BMP180_COM_PRESSURE2 = 0xB4;

        /// <summary>
        /// 8-bit I2C command for the Ultra High Resolution Mode Pressure reading of the BMP180
        /// </summary>
        private const byte BMP180_COM_PRESSURE3 = 0xF4;

        /// <summary>
        /// 8-bit I2C command for software reset the BMP180
        /// </summary>
        private const byte BMP180_COM_SOFTRESET = 0xE0;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_AC1 = 0xAA;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_AC2 = 0xAC;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_AC3 = 0xAE;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns> 
        private const byte BMP180_CAL_AC4 = 0xB0;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_AC5 = 0xB2;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_AC6 = 0xB4;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_B1 = 0xB6;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_B2 = 0xB8;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_MB = 0xBA;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_MC = 0xBC;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_MD = 0xBE;

        public Bmp180CalibrationData CalibrationData { get; private set; }

        private double x0, x1, x2, y0, y1, y2, p0, p1, p2;
        private static TimeSpan DEFAULT_READ_INTERVAL = TimeSpan.FromSeconds(3);

        private double? currentTemperature;
        public double? CurrentTemperature
        {
            get
            {
                if (Mode == ReadingMode.Manual)
                    throw new NotSupportedException($"{nameof(CurrentTemperature)} is available only when {nameof(Mode)} is set to {ReadingMode.Continuous}.");

                return currentTemperature;
            }
        }

        private double? currentPressure;
        public double? CurrentPressure
        {
            get
            {
                if (Mode == ReadingMode.Manual)
                    throw new NotSupportedException($"{nameof(CurrentPressure)} is available only when {nameof(Mode)} is set to {ReadingMode.Continuous}.");

                return currentPressure;
            }
        }

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public event EventHandler ReadingChanged;

        public Bmp180BarometricSensor(int slaveAddress = DefaultI2cAddress, ReadingMode mode = ReadingMode.Continuous, I2cBusSpeed busSpeed = I2cBusSpeed.StandardMode, I2cSharingMode sharingMode = I2cSharingMode.Shared, string i2cControllerName = RaspberryPiI2cControllerName)
            : base(slaveAddress, mode, DEFAULT_READ_INTERVAL, busSpeed, sharingMode, i2cControllerName)
        { }

        protected override Task InitializeSensorAsync()
        {
            this.ReadCalibrationData();

            base.InitializeTimer();
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Retrieve calibration data from device
        /// </summary>
        private void ReadCalibrationData()
        {
            var data = WriteRead(BMP180_CAL_AC1, 2);
            Array.Reverse(data);
            CalibrationData.AC1 = BitConverter.ToInt16(data, 0);

            data = WriteRead(BMP180_CAL_AC2, 2);
            Array.Reverse(data);
            CalibrationData.AC2 = BitConverter.ToInt16(data, 0);

            data = WriteRead(BMP180_CAL_AC3, 2);
            Array.Reverse(data);
            CalibrationData.AC3 = BitConverter.ToInt16(data, 0);

            data = WriteRead(BMP180_CAL_AC4, 2);
            Array.Reverse(data);
            CalibrationData.AC4 = BitConverter.ToUInt16(data, 0);

            data = WriteRead(BMP180_CAL_AC5, 2);
            Array.Reverse(data);
            CalibrationData.AC5 = BitConverter.ToUInt16(data, 0);

            data = WriteRead(BMP180_CAL_AC6, 2);
            Array.Reverse(data);
            CalibrationData.AC6 = BitConverter.ToUInt16(data, 0);

            data = WriteRead(BMP180_CAL_B1, 2);
            Array.Reverse(data);
            CalibrationData.B1 = BitConverter.ToInt16(data, 0);

            data = WriteRead(BMP180_CAL_B2, 2);
            Array.Reverse(data);
            CalibrationData.B2 = BitConverter.ToInt16(data, 0);

            data = WriteRead(BMP180_CAL_MB, 2);
            Array.Reverse(data);
            CalibrationData.MB = BitConverter.ToInt16(data, 0);

            data = WriteRead(BMP180_CAL_MC, 2);
            Array.Reverse(data);
            CalibrationData.MC = BitConverter.ToInt16(data, 0);

            data = WriteRead(BMP180_CAL_MD, 2);
            Array.Reverse(data);
            CalibrationData.MD = BitConverter.ToInt16(data, 0);

            // Compute floating-point polynominals
            var c3 = 160.0 * Math.Pow(2, -15) * CalibrationData.AC3;
            var c4 = Math.Pow(10, -3) * Math.Pow(2, -15) * CalibrationData.AC4;
            var b1 = Math.Pow(160, 2) * Math.Pow(2, -30) * CalibrationData.B1;
            var c5 = (Math.Pow(2, -15) / 160) * CalibrationData.AC5;
            var c6 = CalibrationData.AC6;
            var mc = (Math.Pow(2, 11) / Math.Pow(160, 2)) * CalibrationData.MC;
            var md = CalibrationData.MD / 160.0;
            x0 = CalibrationData.AC1;
            x1 = 160.0 * Math.Pow(2, -13) * CalibrationData.AC2;
            x2 = Math.Pow(160, 2) * Math.Pow(2, -25) * CalibrationData.B2;
            y0 = c4 * Math.Pow(2, 15);
            y1 = c4 * c3;
            y2 = c4 * b1;
            p0 = (3791.0 - 8.0) / 1600.0;
            p1 = 1.0 - 7357.0 * Math.Pow(2, -20);
            p2 = 3038.0 * 100.0 * Math.Pow(2, -36);
        }

        public async Task<byte[]> ReadUncompestatedTemperatureAsync()
        {
            var command = new[] { BMP180_REG_CONTROL, BMP180_COM_TEMPERATURE };
            Device.Write(command);
            await Task.Delay(5);
            return WriteRead(BMP180_REG_RESULT, 2); ;
        }

        public async Task<byte[]> ReadUncompestatedPressureAsync(Bmp180AccuracyMode ossMode)
        {
            byte presssureCommand = 0;
            var delay = 5;

            switch (ossMode)
            {
                case Bmp180AccuracyMode.UltraLowPower:
                    presssureCommand = BMP180_COM_PRESSURE0;
                    delay = 5;
                    break;
                case Bmp180AccuracyMode.Standard:
                    presssureCommand = BMP180_COM_PRESSURE1;
                    delay = 8;
                    break;
                case Bmp180AccuracyMode.HighResolution:
                    presssureCommand = BMP180_COM_PRESSURE2;
                    delay = 14;
                    break;
                case Bmp180AccuracyMode.UltraHighResolution:
                    presssureCommand = BMP180_COM_PRESSURE3;
                    delay = 26;
                    break;
            }

            var command = new[] { BMP180_REG_CONTROL, presssureCommand };
            Device.Write(command);

            await Task.Delay(delay);

            return WriteRead(BMP180_REG_RESULT, 3);
        }

        private byte[] WriteRead(byte reg, int readLength)
        {
            var readBuffer = new byte[readLength];
            Device.WriteRead(new[] { reg }, readBuffer);

            return readBuffer;
        }

        private int CalculateB5(int ut)
        {
            var X1 = (ut - CalibrationData.AC6) * (CalibrationData.AC5) >> 15;
            var X2 = (CalibrationData.MC << 11) / (X1 + CalibrationData.MD);
            return X1 + X2;
        }

        public async Task<SensorData> GetSensorDataAsync(Bmp180AccuracyMode oss = Bmp180AccuracyMode.Standard)
        {
            // Create the return object.
            var sensorData = new SensorData();

            // Read the Uncompestated values from the sensor.
            var tData = await ReadUncompestatedTemperatureAsync();
            var pData = await ReadUncompestatedPressureAsync(oss);

            // Keep raw data for debug
            sensorData.UncompestatedTemperature = tData;
            sensorData.UncompestatedPressure = pData;

            var ut = (tData[0] << 8) + tData[1];
            var up = (pData[0] * 256.0) + pData[1] + (pData[2] / 256.0);

            // Calculate real values
            var b5 = this.CalculateB5(ut);

            var t = (b5 + 8) >> 4;
            sensorData.Temperature = t / 10.0;

            var s = sensorData.Temperature - 25.0;
            var x = (x2 * Math.Pow(s, 2)) + (x1 * s) + x0;
            var y = (y2 * Math.Pow(s, 2)) + (y1 * s) + y0;
            var z = (up - x) / y;

            sensorData.Pressure = (p2 * Math.Pow(z, 2)) + (p1 * z) + p0;

            return sensorData;
        }

        protected override async void OnTimer()
        {
            // Retrive luminosity value.
            var sensorData = await this.GetSensorDataAsync();

            if (sensorData.Pressure != currentPressure || sensorData.Temperature != currentTemperature)
            {
                currentPressure = sensorData.Pressure;
                currentTemperature = sensorData.Temperature;

                RaiseEventHelper.CheckRaiseEventOnUIThread(this, ReadingChanged, RaiseEventsOnUIThread);
            }
        }       

        public override void Dispose()
        {
            // Powerdown the sensor.
            base.Dispose();
        }

        public class Bmp180CalibrationData
        {
            public short AC1 { get; set; }

            public short AC2 { get; set; }

            public short AC3 { get; set; }

            public ushort AC4 { get; set; }

            public ushort AC5 { get; set; }

            public ushort AC6 { get; set; }

            public short B1 { get; set; }

            public short B2 { get; set; }

            public short MB { get; set; }

            public short MC { get; set; }

            public short MD { get; set; }
        }

        public class SensorData
        {
            public double Temperature { get; set; }

            public double Pressure { get; set; }

            public byte[] UncompestatedTemperature { get; set; }

            public byte[] UncompestatedPressure { get; set; }
        }
    }
}
