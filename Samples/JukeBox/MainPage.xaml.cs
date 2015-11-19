using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Usb;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace JukeBox
{
    public sealed partial class MainPage : Page
    {
        private readonly DeviceWatcher deviceWatcher = null;
        private readonly Dictionary<string, IStorageFile> audioFiles;

        private bool isBusy;

        private readonly RemoteConnection connection;
        private readonly Random random;

        private const string CONNECT = "Connect an USB drive to the board";
        private const string DRIVE_FOUND = "USB drive connected. Found {0} MP3 audio files";

        public MainPage()
        {
            this.InitializeComponent();

            connection = new RemoteConnection();
            connection.OnGetMusicEvent(OnGetMusic)
                .OnPlayMusicEvent(OnPlayMusic)
                .OnPauseMusicEvent(OnPauseMusic)
                .OnStopMusicEvent(OnStopMusic)
                .OnPlayRandomMusicEvent(OnPlayRandomMusic);

            deviceWatcher = DeviceInformation.CreateWatcher(DeviceClass.PortableStorageDevice);
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            audioFiles = new Dictionary<string, IStorageFile>();
            random = new Random(unchecked((int)(DateTime.Now.Ticks)));
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await connection.ConnectAsync();
            deviceWatcher.Start();

            message.Text = CONNECT;

            base.OnNavigatedTo(e);
        }

        private void DeviceWatcher_Stopped(DeviceWatcher sender, object args)
        { }

        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        { }

        private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
           => await this.RefreshAudioFilesAsync();

        private async void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
             => await this.ClearAudioFilesAsync();

        private async Task RefreshAudioFilesAsync()
        {
            if (!isBusy)
            {
                isBusy = true;

                var folders = await KnownFolders.RemovableDevices.GetFoldersAsync();
                foreach (var storageFolder in folders)
                {
                    var files = await storageFolder.GetFilesAsync();

                    foreach (var file in files)
                        audioFiles[file.Name] = file;
                }

                if (audioFiles.Count > 0)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () => message.Text = string.Format(DRIVE_FOUND, audioFiles.Count));

                    await connection.SendAvailableMusicAsync(audioFiles.Keys);
                }

                isBusy = false;
            }
        }

        private async Task ClearAudioFilesAsync()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => message.Text = CONNECT);

            audioFiles.Clear();
            await connection.SendAvailableMusicAsync(audioFiles.Keys);
        }

        private async void OnGetMusic()
            => await connection.SendAvailableMusicAsync(audioFiles.Keys);

        private async void OnPlayMusic(string fileName)
            => await this.PlayAsync(fileName);

        private void OnPauseMusic()
        {
            if (mediaPlayer.CurrentState == MediaElementState.Paused)
                mediaPlayer.Play();
            else
                mediaPlayer.Pause();
        }

        private void OnStopMusic() => mediaPlayer.Stop();

        private async void OnPlayRandomMusic()
        {
            var audioFile = audioFiles.ElementAt(random.Next(0, audioFiles.Count));
            await this.PlayAsync(audioFile.Key);
        }

        private async Task PlayAsync(string fileName)
        {
            if (audioFiles.ContainsKey(fileName))
            {
                var audioFile = audioFiles[fileName];
                var stream = await audioFile.OpenAsync(FileAccessMode.Read);
                mediaPlayer.SetSource(stream, audioFile.ContentType);
                mediaPlayer.Play();
            }
        }
    }
}
