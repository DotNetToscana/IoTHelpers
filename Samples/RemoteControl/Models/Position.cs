using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControl.Models
{
    public class Position
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double Accuracy { get; set; }

        public DateTimeOffset Timestamp { get; set; }
    }
}
