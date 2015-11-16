using IoTService.Hubs;
using IoTService.Models;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IoTService.Api
{
    [RoutePrefix("api")]
    public class JukeboxController : ApiController
    {
        private readonly static Lazy<IHubContext<ISensorHub>> sensorHub =
                new Lazy<IHubContext<ISensorHub>>(
                () => GlobalHost.ConnectionManager.GetHubContext<SensorHub, ISensorHub>());

        [Route("jukebox/get", Name = "GetMusic")]
        [HttpGet]
        public void GetMusic()
        {
            // Chiede al client la lista delle musiche.
            sensorHub.Value.Clients.Group(SensorHub.DEVICES_GROUP).GetMusic();
        }

        [Route("jukebox/play", Name = "PlayMusic")]
        [HttpPost]
        public void PlayMusic(string fileName)
        {
            // Invia una notifica SignalR con il file da riprodurre.
            sensorHub.Value.Clients.Group(SensorHub.DEVICES_GROUP).PlayMusic(fileName);
        }

        [Route("jukebox/pause", Name = "PauseMusic")]
        [HttpPost]
        public void PauseMusic()
        {
            // Invia una notifica SignalR per mettere in pausa la riproduzione.
            sensorHub.Value.Clients.Group(SensorHub.DEVICES_GROUP).PauseMusic();
        }

        [Route("jukebox/stop", Name = "StopMusic")]
        [HttpPost]
        public void StopMusic()
        {
            // Invia una notifica SignalR per fermare la riproduzione.
            sensorHub.Value.Clients.Group(SensorHub.DEVICES_GROUP).StopMusic();
        }

        [Route("jukebox/random", Name = "PlayRandomMusic")]
        [HttpPost]
        public void PlayRandomMusic()
        {
            // Invia una notifica SignalR per fermare la riproduzione.
            sensorHub.Value.Clients.Group(SensorHub.DEVICES_GROUP).PlayRandomMusic();
        }
    }
}
