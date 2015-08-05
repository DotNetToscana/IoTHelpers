using Microsoft.AspNet.SignalR.Client;
using RemoteControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace RemoteControl
{
    public class RemoteConnection : IDisposable
    {
        private const string SERVICE_URL = "http://iotserviceweb.azurewebsites.net/";

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
			if (string.IsNullOrWhiteSpace(SERVICE_URL))
				throw new Exception("Service URL not specified.");

            hubConnection = new HubConnection(SERVICE_URL);
            hubProxy = hubConnection.CreateHubProxy(HUB_NAME);
        }

        public async Task ConnectAsync()
        {
            await hubConnection.Start();

            // Register the device.
            await hubProxy.Invoke(ADD_DEVICE_METHOD);

            // Register SignalR events.
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
