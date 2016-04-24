using IoTHelpers.Boards;
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

namespace MotionDetector
{
    public sealed partial class MainPage : Page
    {
        private MulticolorLed led;
        private Sr501PirMotionDetector pir;

        public MainPage()
        {
            InitializeComponent();
            Unloaded += MainPage_Unloaded;

            led = new MulticolorLed(redPinNumber: 27, greenPinNumber: 22, bluePinNumber: 23);

            pir = new Sr501PirMotionDetector(pinNumber: 5);
            pir.RaiseEventsOnUIThread = true;
            pir.MotionDetected += pir_MotionDetected;
            pir.MotionStopped += pir_MotionStopped;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var board = RaspberryPi2Board.GetDefault();
            if (board != null)
            {
                board.PowerLed.TurnOff();
                board.StatusLed.TurnOn();
            }

            base.OnNavigatedTo(e);
        }

        private void pir_MotionDetected(object sender, EventArgs e)
        {
            // Starts the alarm.
            led.TurnRed();
            alarm.Play();
        }

        private void pir_MotionStopped(object sender, EventArgs e)
        {
            // Stops the alarm.
            led.TurnOff();
            alarm.Stop();
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // Cleanup.
            if (pir != null)
            {
                pir.MotionDetected -= pir_MotionDetected;
                pir.MotionStopped -= pir_MotionStopped;
                pir.Dispose();
            }

            if (led != null)
                led.Dispose();
        }
    }
}
