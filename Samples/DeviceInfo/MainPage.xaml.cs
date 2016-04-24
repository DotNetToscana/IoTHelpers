using IoTHelpers.Boards;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    }
}
