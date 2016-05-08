using RoverClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Gaming.Input;

namespace RoverClient.Services
{
    public static class GamepadService
    {
        public static bool Autopiloting { get; set; }

        private const double DIRECTION_THRESHOLD = 0.6;

        public static RoverMovementType? GetCurrentCommand()
        {
            var controller = Gamepad.Gamepads.FirstOrDefault();
            if (controller != null)
            {
                var command = RoverMovementType.Stop;
                var reading = controller.GetCurrentReading();

                if (reading.Buttons.HasFlag(GamepadButtons.X))
                {
                    Autopiloting = false;
                    command = RoverMovementType.Stop;
                }
                else if (reading.Buttons.HasFlag(GamepadButtons.A) || Autopiloting)
                {
                    Autopiloting = true;
                    command = RoverMovementType.Autopilot;
                }
                else
                {
                    // Checks movement on the Y axis. We use a threshold to normalize values.
                    if (reading.LeftThumbstickY.Between(-1, -DIRECTION_THRESHOLD))
                        command = RoverMovementType.Backward;
                    else if (reading.LeftThumbstickY.Between(DIRECTION_THRESHOLD, 1))
                        command = RoverMovementType.Forward;

                    // Checks movement on the X axis. We use a threshold to normalize values.
                    if (reading.LeftThumbstickX.Between(-1, -DIRECTION_THRESHOLD))
                    {
                        switch (command)
                        {
                            case RoverMovementType.Forward:
                                command = RoverMovementType.ForwardLeft;
                                break;
                            case RoverMovementType.Backward:
                                command = RoverMovementType.BackwardLeft;
                                break;

                            case RoverMovementType.Stop:
                            default:
                                command = RoverMovementType.RotateLeft;
                                break;
                        }
                    }
                    else if (reading.LeftThumbstickX.Between(DIRECTION_THRESHOLD, 1))
                    {
                        switch (command)
                        {
                            case RoverMovementType.Forward:
                                command = RoverMovementType.ForwardRight;
                                break;
                            case RoverMovementType.Backward:
                                command = RoverMovementType.BackwardRight;
                                break;

                            case RoverMovementType.Stop:
                            default:
                                command = RoverMovementType.RotateRight;
                                break;
                        }
                    }
                }

                return command;
            }

            return null;
        }

        private static bool Between<T>(this T value, T from, T to) where T : IComparable<T>
            => value.CompareTo(from) >= 0 && value.CompareTo(to) <= 0;
    }
}
