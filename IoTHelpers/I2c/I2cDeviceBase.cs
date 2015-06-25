using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace IoTHelpers.I2c
{
    public class I2cDeviceBase : IDisposable
    {
        // I2C Controller name
        public const string I2C_CONTROLLER_NAME = "I2C1";

        public I2cConnectionSettings Settings { get; }

        // I2C Device
        public I2cDevice Device { get; private set; }

        private string deviceSelector;

        public I2cDeviceBase(int slaveAddress, I2cBusSpeed busSpeed = I2cBusSpeed.FastMode, I2cSharingMode sharingMode = I2cSharingMode.Shared, string i2cControllerName = I2C_CONTROLLER_NAME)
        {
            // Initialize I2C device
            Settings = new I2cConnectionSettings(slaveAddress)
            {
                BusSpeed = busSpeed,
                SharingMode = sharingMode
            };

            deviceSelector = I2cDevice.GetDeviceSelector(i2cControllerName);    /* Find the selector string for the I2C bus controller */
        }

        public async Task InitializeAsync()
        {
            var dis = await DeviceInformation.FindAllAsync(deviceSelector);     /* Find the I2C bus controller device with our selector string  */
            Device = await I2cDevice.FromIdAsync(dis[0].Id, Settings);          /* Create an I2cDevice with our selected bus controller and I2C settings */

            await InitializeSensorAsync();
        }

        protected virtual Task InitializeSensorAsync() { return Task.FromResult<object>(null); }

        public virtual void Dispose()
        {
            if (Device != null)
                Device.Dispose();
        }
    }
}
