using Humiture.Models;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Humiture
{
    public class RemoteConnection : IDisposable
    {
        private const string SERVICE_URL = "http://localhost:37309/";

        private const string HUB_NAME = "SensorHub";

        private const string ADD_DEVICE_METHOD = "AddDevice";
        private const string HUMITURE_CHANGED_EVENT = "HumitureChanged";

        private readonly HubConnection hubConnection;
        private readonly IHubProxy hubProxy;

        public Task SendHumiture(double humidity, double temperature)
        {
            if (hubConnection.State == ConnectionState.Connected)
            {
                return hubProxy.Invoke(HUMITURE_CHANGED_EVENT, new HumitureData
                {
                    Temperature = temperature,
                    Humidity = humidity
                });
            }

            return Task.CompletedTask;
        }

        public RemoteConnection()
        {
            if (string.IsNullOrWhiteSpace(SERVICE_URL))
            {
                throw new ArgumentNullException("Service URL not specified.");
            }

            hubConnection = new HubConnection(SERVICE_URL);
            hubProxy = hubConnection.CreateHubProxy(HUB_NAME);
        }

        public async Task ConnectAsync()
        {
            await hubConnection.Start();

            // Register the device.
            await hubProxy.Invoke(ADD_DEVICE_METHOD);
        }

        public void Dispose()
        {
            hubConnection.Dispose();
        }
    }
}
