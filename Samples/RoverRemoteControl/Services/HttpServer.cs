using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using Windows.Foundation.Collections;

namespace RoverRemoteControl.Services
{
    public sealed class HttpServer : IDisposable
    {
        private const string htmlString = "<html><head><meta http-equiv=\"refresh\" content=\"1\"><title>Streaming</title></head><body><img src='data:image/jpeg;base64,{0}' /></body></html>";

        private readonly int port;
        private readonly StreamSocketListener listener;

        private Func<Task<byte[]>> onRequestDataAsync;
        public HttpServer OnRequestDataAsync(Func<Task<byte[]>> action)
        {
            onRequestDataAsync = action;
            return this;
        }

        public HttpServer(int serverPort)
        {
            listener = new StreamSocketListener();
            listener.Control.KeepAlive = true;
            listener.Control.NoDelay = true;

            port = serverPort;
            listener.ConnectionReceived += async (s, e) => { await ProcessRequestAsync(e.Socket); };
        }

        public Task StartServerAsync() => listener.BindServiceNameAsync(port.ToString()).AsTask();

        public Task StopServerAsync() => listener.CancelIOAsync().AsTask();

        private async Task ProcessRequestAsync(StreamSocket socket)
        {
            var content = await onRequestDataAsync.Invoke();
            var base64 = Convert.ToBase64String(content);

            var html = string.Format(htmlString, base64);
            var bodyArray = Encoding.UTF8.GetBytes(html);

            // Shows the html 
            using (var outputStream = socket.OutputStream)
            {
                using (var response = outputStream.AsStreamForWrite())
                {
                    using (var stream = new MemoryStream(bodyArray))
                    {
                        var header = "HTTP/1.1 200 OK\r\n" +
                                            $"Content-Length: {stream.Length}\r\n" +
                                            "Connection-Type: text/html\r\n" +
                                            "Connection: close\r\n\r\n";
                                            
                        var headerArray = Encoding.UTF8.GetBytes(header);
                        response.Write(headerArray, 0, headerArray.Length);
                        stream.CopyTo(response);
                        response.Flush();
                    }
                }
            }
        }

        public void Dispose()
        {
            var task = this.StopServerAsync();
            listener.Dispose();
        }
    }
}
