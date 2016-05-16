using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace RoverClient.Services
{
    public class SocketConnection : IDisposable
    {
        private const int REMOTE_PORT = 2045;
        private readonly StreamSocket socket;
        private DataWriter writer;

        private readonly int port;

        public SocketConnection(int serverPort = REMOTE_PORT)
        {
            socket = new StreamSocket();

            // If necessary, tweak the socket's control options before carrying out the connect operation.
            // Refer to the StreamSocketControl class' MSDN documentation for the full list of control options.
            socket.Control.KeepAlive = false;

            port = serverPort;
        }

        public async Task ConnectAsync(string hostName)
        {
            await socket.ConnectAsync(new HostName(hostName), REMOTE_PORT.ToString());
            writer = new DataWriter(socket.OutputStream);
        }

        public async Task SendAsync(string message)
        {
            writer.WriteUInt32(writer.MeasureString(message));
            writer.WriteString(message);
            await writer.StoreAsync();
        }

        public async Task CloseAsync()
        {
            writer.Dispose();
            await socket.CancelIOAsync();
        }

        public void Dispose()
        {
            var task = this.CloseAsync();
            socket.Dispose();
        }
    }
}
