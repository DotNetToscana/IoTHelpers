using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoverRemoteControl.Models
{
    public enum RoverMovementType
    {
        Forward = 1,
        Backward = 2,
        TurnLeft = 3,
        TurnRight = 4,
        RotateLeft = 5,
        RotateRight = 6,
        Autopilot = 7,
        Stop = 8
    }

    public class RoverMovement
    {
        public RoverMovementType Movement { get; set; }

        public override string ToString() => Movement.ToString();
    }
}
