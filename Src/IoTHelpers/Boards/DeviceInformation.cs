using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace IoTHelpers.Boards
{
    public enum DeviceType
    {
        RaspberryPi2,
        RaspberryPi3,
        MinnowBoardMax,
        DragonBoard410c,
        Colibri,
        GenericBoard,
        Unknown
    };

    public static class DeviceInformation
    {
        public static DeviceType Type { get; } = DeviceType.Unknown;

        public static string DeviceName { get; }

        public static Guid Id { get; }

        public static string ProductName { get; }

        static DeviceInformation()
        {
            var deviceInfo = new EasClientDeviceInformation();

            Id = deviceInfo.Id;
            DeviceName = deviceInfo.FriendlyName;
            ProductName = deviceInfo.SystemProductName;

            if (ProductName.IndexOf("MinnowBoard", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Type = DeviceType.MinnowBoardMax;
            }
            else if (ProductName.IndexOf("Raspberry", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (ProductName.IndexOf("Pi 3", StringComparison.OrdinalIgnoreCase) >= 0)
                    Type = DeviceType.RaspberryPi3;
                else
                    Type = DeviceType.RaspberryPi2;
            }
            else if (ProductName == "SBC")
            {
                Type = DeviceType.DragonBoard410c;
            }
            else if (ProductName.IndexOf("Cardhu", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Type = DeviceType.Colibri;
            }
            else
            {
                Type = DeviceType.GenericBoard;
            }
        }

        public static string GetIpAddress()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp?.NetworkAdapter != null)
            {
                var hostname = NetworkInformation.GetHostNames()
                        .SingleOrDefault(hn => hn.IPInformation?.NetworkAdapter?.NetworkAdapterId == icp.NetworkAdapter.NetworkAdapterId);

                return hostname?.CanonicalName;
            }

            return null;
        }
    }
}

