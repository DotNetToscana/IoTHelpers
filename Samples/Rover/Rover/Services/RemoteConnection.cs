using Microsoft.AspNet.SignalR.Client;
using Rover.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Rover.Services
{
    public class RemoteConnection : IDisposable
    {
        private const string SERVICE_URL = "http://localhost:37309/";

        private const string HUB_NAME = "SensorHub";

        private const string ADD_DEVICE_METHOD = "AddDevice";
        private const string ROVER_MOVE_EVENT = "MoveRover";

        private readonly HubConnection hubConnection;
        private readonly IHubProxy hubProxy;

        private Action<RoverMovement> movementEvent;
        public RemoteConnection OnRoverMovementEvent(Action<RoverMovement> action)
        {
            movementEvent = action;
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
            hubProxy.On<RoverMovement>(ROVER_MOVE_EVENT, async (movement) =>
                {
                    var dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;
                    if (dispatcher != null)
                        await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => movementEvent?.Invoke(movement));
                    else
                        movementEvent?.Invoke(movement);
                });
        }

        public void Dispose()
        {
            hubConnection.Dispose();
        }
    }
}
