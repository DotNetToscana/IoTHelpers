using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControl.Models
{
    public class TelemetryData
    {
        public string DeviceId { get; set; }

        public double Temperature { get; set; }

        public double Humidity { get; set; }

        public double LightLevel { get; set; }

        public Position Position { get; set; }
    }
}
