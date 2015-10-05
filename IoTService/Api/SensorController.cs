using IoTService.Hubs;
using IoTService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IoTService.Api
{
    [RoutePrefix("api")]
    public class SensorController : ApiController
    {
        private readonly static Lazy<Microsoft.AspNet.SignalR.IHubContext<ISensorHub>> sensorHub =
                new Lazy<Microsoft.AspNet.SignalR.IHubContext<ISensorHub>>(
                () => Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<SensorHub, ISensorHub>());

        [Route("led/set", Name = "SetLed")]
        [HttpPost]
        public void SetLed(Rgb rgb)
        {
            // Invia una notifica SignalR con i nuovi valori del LED.
            sensorHub.Value.Clients.Group(SensorHub.DEVICES_GROUP).SetLed(rgb);
        }

        [Route("rover/move", Name = "MoveRover")]
        [HttpPost]
        public void MoveRemove(RoverMovement movement)
        {
            // Invia una notifica SignalR con il comando di movimento del Rover.
            sensorHub.Value.Clients.Group(SensorHub.DEVICES_GROUP).MoveRover(movement);
        }
    }
}
