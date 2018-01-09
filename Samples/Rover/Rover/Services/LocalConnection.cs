using Rover.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Rover.Services
{
    public class LocalConnection : IDisposable
    {
        private const int LOCAL_PORT = 2045;
        private readonly StreamSocketListener listener;

        private readonly int port;

        private Action<RoverMovement> movementEvent;
        public LocalConnection OnRoverMovementEvent(Action<RoverMovement> action)
        {
            movementEvent = action;
            return this;
        }

        public LocalConnection(int serverPort = LOCAL_PORT)
        {
            listener = new StreamSocketListener();
            listener.ConnectionReceived += OnConnection;

            // If necessary, tweak the listener's control options before carrying out the bind operation.
            // These options will be automatically applied to the connected StreamSockets resulting from
            // incoming connections (i.e., those passed as arguments to the ConnectionReceived event handler).
            // Refer to the StreamSocketListenerControl class' MSDN documentation for the full list of control options.
            listener.Control.KeepAlive = false;

            port = serverPort;
        }

        public Task StartServerAsync() => listener.BindServiceNameAsync(port.ToString()).AsTask();

        public Task StopServerAsync() => listener.CancelIOAsync().AsTask();

        /// <summary>
        /// Invoked once a connection is accepted by StreamSocketListener.
        /// </summary>
        /// <param name="sender">The listener that accepted the connection.</param>
        /// <param name="args">Parameters associated with the accepted connection.</param>
        private async void OnConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Debug.WriteLine("Socket connection established.");

            var stopped = false;
            var reader = new DataReader(args.Socket.InputStream);

            try
            {
                while (!stopped)
                {
                    var movementType = RoverMovementType.Stop;

                    // Read first 4 bytes (length of the subsequent string).
                    var sizeFieldCount = await reader.LoadAsync(sizeof(uint));
                    if (sizeFieldCount != sizeof(uint))
                    {
                        // The underlying socket was closed before we were able to read the whole data.
                        stopped = true;
                    }

                    if (!stopped)
                    {
                        // Read the string.
                        var stringLength = reader.ReadUInt32();
                        var actualStringLength = await reader.LoadAsync(stringLength);
                        if (stringLength != actualStringLength)
                        {
                            // The underlying socket was closed before we were able to read the whole data.
                            stopped = true;
                        }
                        else
                        {
                            var message = reader.ReadString(actualStringLength);
                            movementType = (RoverMovementType)Enum.Parse(typeof(RoverMovementType), message);
                        }
                    }

                    movementEvent?.Invoke(new RoverMovement(movementType));
                }

                // The connection has been stopped.
                movementEvent?.Invoke(new RoverMovement(RoverMovementType.Stop));
            }
            catch
            {
                // Exception: sends the stop command.
                movementEvent?.Invoke(new RoverMovement(RoverMovementType.Stop));
            }
        }

        public void Dispose()
        {
            var task = StopServerAsync();
            listener.Dispose();
        }
    }
}
