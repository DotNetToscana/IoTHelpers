using IoTHelpers.Boards;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace DeviceInfo
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            deviceIdTextBlock.Text = DeviceInformation.Id.ToString();
            deviceNameTextBlock.Text = DeviceInformation.DeviceName;
            productNameTextBlock.Text = DeviceInformation.ProductName;
            deviceTypeTextBlock.Text = DeviceInformation.Type.ToString();
            ipAddressTextBlock.Text = DeviceInformation.GetIpAddress();

            base.OnNavigatedTo(e);
        }

        private async void shutdownButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Are you sure you want to shutdown the device? You can't turn it on remotely.", "IoTHelpers");
            dialog.Commands.Add(new UICommand("Yes", (cmd) => ShutdownManager.Shutdown()));
            dialog.Commands.Add(new UICommand("No"));
            dialog.CancelCommandIndex = 1;

            await dialog.ShowAsync();
        }

        private async void restartButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Are you sure you want to restart the device?", "IoTHelpers");
            dialog.Commands.Add(new UICommand("Yes", (cmd) => ShutdownManager.Restart()));
            dialog.Commands.Add(new UICommand("No"));
            dialog.CancelCommandIndex = 1;

            await dialog.ShowAsync();
        }
    }
}
