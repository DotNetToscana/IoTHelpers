using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoverClient.Models
{
    public enum RoverMovementType
    {
        Forward = 1,
        ForwardLeft = 2,
        ForwardRight = 3,
        Backward = 4,
        BackwardLeft = 5,
        BackwardRight = 6,
        RotateLeft = 7,
        RotateRight = 8,
        Autopilot = 9,
        Stop = 10
    }
}
