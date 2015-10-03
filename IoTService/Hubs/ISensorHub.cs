using IoTService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IoTService.Hubs
{
    public interface ISensorHub
    {
        void SetLed(Rgb rgb);

        void HumitureChanged(Humiture humiture);
    }
}