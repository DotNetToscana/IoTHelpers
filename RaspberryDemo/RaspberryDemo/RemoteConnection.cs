using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;
using RaspberryDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace RaspberryDemo
{
    public class RemoteConnection : IDisposable
    {
        //public const string ServiceUrl = "http://localhost:37309/";
        public const string SERVICE_URL = "http://iotserviceweb.azurewebsites.net/";

        private const string HUB_NAME = "SensorHub";
        private const string ADD_DEVICE_METHOD = "AddDevice";
        public const string SET_LED_EVENT = "SetLed";

        private readonly HubConnection hubConnection;
        private readonly IHubProxy hubProxy;

        private Action<Rgb> ledEvent;
        public RemoteConnection OnLedEvent(Action<Rgb> action)
        {
            ledEvent = action;
            return this;
        }

        public RemoteConnection()
        {
            hubConnection = new HubConnection(SERVICE_URL);
            hubProxy = hubConnection.CreateHubProxy(HUB_NAME);
        }

        public async Task ConnectAsync()
        {
            await hubConnection.Start();

            // Registra il device.
            await hubProxy.Invoke(ADD_DEVICE_METHOD);

            // Registra gli eventi che sono ricevuti tramite SignalR.
            hubProxy.On<Rgb>(SET_LED_EVENT, async (rgb) =>
                {
                    var dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;
                    if (dispatcher != null)
                        await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ledEvent?.Invoke(rgb));
                    else
                        ledEvent?.Invoke(rgb);
                });
        }

        public void Dispose()
        {
            hubConnection.Dispose();
        }
    }
}
