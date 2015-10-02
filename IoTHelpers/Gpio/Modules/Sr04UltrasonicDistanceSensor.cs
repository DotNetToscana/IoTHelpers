using IoTHelpers.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Gpio.Modules
{
    public class Sr04UltrasonicDistanceSensor : GpioModuleBase
    {
        private readonly Timer timer;

        private readonly GpioModule triggerPin;
        private readonly GpioModule echoPin;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        public double? CurrentDistance { get; private set; }

        public event EventHandler DistanceChanged;

        public Sr04UltrasonicDistanceSensor(int triggerPinNumber, int echoPinNumber)
        {
            triggerPin = new GpioModule(Controller, triggerPinNumber, GpioPinDriveMode.Output);
            echoPin = new GpioModule(Controller, echoPinNumber, GpioPinDriveMode.Input);

            timer = new Timer(CheckState, null, 0, 500);
        }

        private void CheckState(object state)
        {
            this.GetDistance();
            RaiseEventHelper.CheckRaiseEventOnUIThread(this, DistanceChanged, RaiseEventsOnUIThread);
        }

        public double? GetDistance()
        {
            var mre = new ManualResetEvent(false);
            var pulseLength = new Stopwatch();

            //Send pulse
            triggerPin.Pin.Write(GpioPinValue.High);
            mre.WaitOne(TimeSpan.FromMilliseconds(0.01));
            triggerPin.Pin.Write(GpioPinValue.Low);

            //Recieve pusle
            while (echoPin.Pin.Read() == GpioPinValue.Low) ;

            pulseLength.Start();

            while (echoPin.Pin.Read() == GpioPinValue.High) ;

            pulseLength.Stop();

            /* Calculating distance.
               If you take 340 m/sec (approximate speed of sound through air) and convert to cm/sec you get 34000 cm/sec.
               For pulse-echo, the sound travels twice the measured distance so you need to divide the conversion factor 
               by 2 so you get 17000 cm/sec. When you multiply by the measured time, you get distance from the transducer to the object in cm.
            */
            var timeBetween = pulseLength.Elapsed;
            double? distance = timeBetween.TotalSeconds * 17000;

            // Check whether distance is out of range.
            if (timeBetween.TotalMilliseconds < 0.16 || timeBetween.TotalMilliseconds > 38)
                distance = null;

            Debug.WriteLine("Milliseconds: " + timeBetween.TotalMilliseconds);
            Debug.WriteLine("Distance: " + distance);

            if (CurrentDistance != distance)
            {
                CurrentDistance = distance;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, DistanceChanged, RaiseEventsOnUIThread);
            }

            return distance;
        }

        public override void Dispose()
        {
            timer.Dispose();

            if (triggerPin != null)
                triggerPin.Dispose();

            if (echoPin != null)
                echoPin.Dispose();

            base.Dispose();
        }
    }
}

