using IoTHelpers.Gpio.Modules;
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

namespace UltrasonicDistance
{
    public sealed partial class MainPage : Page
    {
        private Sr04UltrasonicDistanceSensor ultrasonic;
        private MulticolorLed led;

        public MainPage()
        {
            this.InitializeComponent();
            Unloaded += MainPage_Unloaded;

            ultrasonic = new Sr04UltrasonicDistanceSensor(triggerPinNumber: 12, echoPinNumber: 16);
            ultrasonic.RaiseEventsOnUIThread = true;
            ultrasonic.DistanceChanged += ultrasonic_DistanceChanged;

            led = new MulticolorLed(redPinNumber: 18, greenPinNumber: 23, bluePinNumber: 24);
        }

        private void ultrasonic_DistanceChanged(object sender, EventArgs e)
        {
            if (ultrasonic.CurrentDistance > 10)
                led.TurnGreen();
            else
                led.TurnRed();
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // Cleanup.
            if (ultrasonic != null)
            {
                ultrasonic.DistanceChanged -= ultrasonic_DistanceChanged;
                ultrasonic.Dispose();
            }

            if (led != null)
                led.Dispose();
        }
    }
}
