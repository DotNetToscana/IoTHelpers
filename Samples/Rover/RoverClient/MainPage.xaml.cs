using RoverClient.Models;
using RoverClient.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.Input;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace RoverClient
{
    public sealed partial class MainPage : Page
    {
        private SocketConnection connection;

        private readonly IPropertySet settings;
        private readonly DispatcherTimer dispatcherTimer;

        private RoverMovementType lastCommand;

        private static TimeSpan TIMER_INTERVAL = TimeSpan.FromMilliseconds(100);
        private const string ROVER_ADDRESS = "RoverAddress";

        public MainPage()
        {
            this.InitializeComponent();

            settings = ApplicationData.Current.RoamingSettings.Values;

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = TIMER_INTERVAL;
            dispatcherTimer.Tick += dispatcherTimer_Tick;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            object address = null;
            if (settings.TryGetValue(ROVER_ADDRESS, out address))
            {
                RoverAddressTextBox.Text = address.ToString();
                connectButton.IsEnabled = true;
            }

            base.OnNavigatedTo(e);
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            try
            {
                await this.DisconnectAsync();
            }
            catch { }

            base.OnNavigatedFrom(e);
        }

        private void RoverAddressTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            connectButton.IsEnabled = !string.IsNullOrWhiteSpace(RoverAddressTextBox.Text);
        }

        private async void connectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RoverAddressTextBox.IsEnabled = false;

                connection = new SocketConnection();
                await connection.ConnectAsync(RoverAddressTextBox.Text.Trim());

                GamepadService.Autopiloting = false;
                dispatcherTimer.Start();

                settings[ROVER_ADDRESS] = RoverAddressTextBox.Text.Trim();

                connectButton.Visibility = Visibility.Collapsed;
                disconnectButton.Visibility = Visibility.Visible;
                roverControl.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                await this.ShowErrorAsync(ex);
                RoverAddressTextBox.IsEnabled = true;
                connection = null;
            }
        }

        private async void disconnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await this.DisconnectAsync();
            }
            catch (Exception ex)
            {
                await this.ShowErrorAsync(ex);
            }
        }

        private async void movementButton_Click(object sender, RoutedEventArgs e)
        {
            var command = (sender as Button).Tag.ToString();
            await this.SendCommandAsync(command);
        }

        private async void dispatcherTimer_Tick(object sender, object args)
        {
            if (connection != null)
            {
                try
                {
                    var command = GamepadService.GetCurrentCommand();

                    if (command != null)
                    {
                        if (lastCommand != command)
                        {
                            Debug.WriteLine(command);
                            lastCommand = command.Value;
                        }

                        var task = this.SendCommandAsync(command.ToString());
                    }
                }
                catch (Exception ex)
                {
                    await this.ShowErrorAsync(ex);
                }
            }
        }

        private async Task SendCommandAsync(string command)
        {
            if (connection != null)
            {
                try
                {
                    await connection.SendAsync(command);
                }
                catch (Exception ex)
                {
                    var error = SocketError.GetStatus(ex.HResult);
                    if (error == SocketErrorStatus.ConnectionResetByPeer)
                        await DisconnectAsync();

                    await this.ShowErrorAsync(ex);
                }
            }
        }

        private async Task DisconnectAsync()
        {
            if (connection != null)
            {
                try
                {
                    dispatcherTimer.Stop();
                    GamepadService.Autopiloting = false;

                    await connection.SendAsync("Stop");
                    await connection.CloseAsync();
                }
                catch { }

                connection = null;

                RoverAddressTextBox.IsEnabled = true;
                connectButton.Visibility = Visibility.Visible;
                disconnectButton.Visibility = Visibility.Collapsed;
                roverControl.Visibility = Visibility.Collapsed;
            }
        }

        private Task ShowErrorAsync(Exception error)
        {
            var message = error.Message;
#if DEBUG
            message += $"{message} ({error.StackTrace})";
#endif
            return new MessageDialog(message).ShowAsync().AsTask();

        }
    }
}
