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

        void MoveRover(RoverMovement movement);

        void MusicAvailable(IEnumerable<string> audioFiles);

        void GetMusic();

        void PlayMusic(string fileName);

        void PlayRandomMusic();

        void PauseMusic();

        void StopMusic();
    }
}