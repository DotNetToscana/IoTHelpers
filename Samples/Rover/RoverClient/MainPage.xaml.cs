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
                await new MessageDialog(ex.Message).ShowAsync();
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
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async void movementButton_Click(object sender, RoutedEventArgs e)
        {
            var command = (sender as Button).Tag.ToString();
            await this.SendCommandAsync(command);
        }

        private void dispatcherTimer_Tick(object sender, object args)
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

        private async Task SendCommandAsync(string command)
        {
            try
            {
                await connection.SendAsync(command);
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async Task DisconnectAsync()
        {
            if (connection != null)
            {
                dispatcherTimer.Stop();
                GamepadService.Autopiloting = false;

                await connection.SendAsync("Stop");
                await connection.CloseAsync();
                connection = null;

                RoverAddressTextBox.IsEnabled = true;
                connectButton.Visibility = Visibility.Visible;
                disconnectButton.Visibility = Visibility.Collapsed;
                roverControl.Visibility = Visibility.Collapsed;
            }
        }
    }
}
