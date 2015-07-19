using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using IoTService.Models;

namespace IoTService.Hubs
{
    public class SensorHub : Hub<ISensorHub>
    {
        public const string DEVICES_GROUP = "device";

        public void AddDevice()
        {
            Groups.Add(Context.ConnectionId, DEVICES_GROUP);
        }
    }
}