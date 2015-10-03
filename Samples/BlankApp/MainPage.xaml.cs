using IoTHelpers.Gpio.Modules;
using RemoteControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace BlankApp
{
    public sealed partial class MainPage : Page
    {
        private RemoteConnection connection;

        public MainPage()
        {
            this.InitializeComponent();

            connection = new RemoteConnection();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //await connection.ConnectAsync();

            base.OnNavigatedTo(e);
        }
    }
}
