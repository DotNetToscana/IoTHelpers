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

namespace JukeBox
{
    public class RemoteConnection : IDisposable
    {
        private const string SERVICE_URL = "http://iotserviceweb.azurewebsites.net/";
        //private const string SERVICE_URL = "http://localhost:37309/";

        private const string HUB_NAME = "SensorHub";

        private const string ADD_DEVICE_METHOD = "AddDevice";

        private const string GET_MUSIC_EVENT = "GetMusic";
        private const string PLAY_MUSIC_EVENT = "PlayMusic";
        private const string PAUSE_MUSIC_EVENT = "PauseMusic";
        private const string STOP_MUSIC_EVENT = "StopMusic";
        private const string PLAY_RANDOM_MUSIC_EVENT = "PlayRandomMusic";
        private const string MUSIC_AVAILABLE_METHOD = "MusicAvailable";

        private readonly HubConnection hubConnection;
        private readonly IHubProxy hubProxy;

        private Action getMusicEvent;
        public RemoteConnection OnGetMusicEvent(Action action)
        {
            getMusicEvent = action;
            return this;
        }

        private Action<string> playMusicEvent;
        public RemoteConnection OnPlayMusicEvent(Action<string> action)
        {
            playMusicEvent = action;
            return this;
        }

        private Action pauseMusicEvent;
        public RemoteConnection OnPauseMusicEvent(Action action)
        {
            pauseMusicEvent = action;
            return this;
        }

        private Action stopMusicEvent;
        public RemoteConnection OnStopMusicEvent(Action action)
        {
            stopMusicEvent = action;
            return this;
        }

        private Action playRandomMusicEvent;
        public RemoteConnection OnPlayRandomMusicEvent(Action action)
        {
            playRandomMusicEvent = action;
            return this;
        }

        public Task SendAvailableMusicAsync(IEnumerable<string> audioFiles)
        {
            if (hubConnection.State == ConnectionState.Connected)
                return hubProxy.Invoke(MUSIC_AVAILABLE_METHOD, audioFiles);

            return Task.FromResult<object>(null);
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

            // Register for SignalR events.
            hubProxy.On(GET_MUSIC_EVENT, async () => await this.RaiseActionAsync(getMusicEvent));
            hubProxy.On<string>(PLAY_MUSIC_EVENT, async (fileName) => await this.RaiseActionAsync(playMusicEvent, fileName));
            hubProxy.On(PAUSE_MUSIC_EVENT, async () => await this.RaiseActionAsync(pauseMusicEvent));
            hubProxy.On(STOP_MUSIC_EVENT, async () => await this.RaiseActionAsync(stopMusicEvent));
            hubProxy.On(PLAY_RANDOM_MUSIC_EVENT, async () => await this.RaiseActionAsync(playRandomMusicEvent));
        }

        private async Task RaiseActionAsync(Action<string> @event, string argument)
        {
            var dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;
            if (dispatcher != null)
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => @event?.Invoke(argument));
            else
                @event?.Invoke(argument);
        }

        private async Task RaiseActionAsync(Action @event)
        {
            var dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;
            if (dispatcher != null)
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => @event?.Invoke());
            else
                @event?.Invoke();
        }

        public void Dispose()
        {
            hubConnection.Dispose();
        }
    }
}
