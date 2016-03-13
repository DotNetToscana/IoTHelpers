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
    public class Sr04UltrasonicDistanceSensor : GpioTimedModuleBase
    {
        private static TimeSpan DEFAULT_READ_INTERVAL = TimeSpan.FromMilliseconds(500);

        private readonly GpioModule triggerPin;
        private readonly GpioModule echoPin;

        public bool RaiseEventsOnUIThread { get; set; } = false;

        private double currentDistance;
        public double CurrentDistance
        {
            get
            {
                if (Mode == ReadingMode.Manual)
                    throw new NotSupportedException($"{nameof(CurrentDistance)} is available only when {nameof(Mode)} is set to {ReadingMode.Continuous}.");

                return currentDistance;
            }
        }

        public event EventHandler DistanceChanged;

        public Sr04UltrasonicDistanceSensor(int triggerPinNumber, int echoPinNumber, ReadingMode mode = ReadingMode.Continuous)
            : base(mode, DEFAULT_READ_INTERVAL)
        {
            triggerPin = new GpioModule(Controller, triggerPinNumber, GpioPinDriveMode.Output);
            echoPin = new GpioModule(Controller, echoPinNumber, GpioPinDriveMode.Input);

            base.InitializeTimer();
        }

        //public double? GetDistance()
        //{
        //    var mre = new ManualResetEvent(false);
        //    var pulseLength = new Stopwatch();

        //    //Send pulse
        //    triggerPin.Pin.Write(GpioPinValue.High);
        //    mre.WaitOne(TimeSpan.FromMilliseconds(0.01));
        //    triggerPin.Pin.Write(GpioPinValue.Low);

        //    //Recieve pusle
        //    while (echoPin.Pin.Read() == GpioPinValue.Low) ;

        //    pulseLength.Start();

        //    while (echoPin.Pin.Read() == GpioPinValue.High) ;

        //    pulseLength.Stop();

        //    /* Calculating distance.
        //       If you take 340 m/sec (approximate speed of sound through air) and convert to cm/sec you get 34000 cm/sec.
        //       For pulse-echo, the sound travels twice the measured distance so you need to divide the conversion factor 
        //       by 2 so you get 17000 cm/sec. When you multiply by the measured time, you get distance from the transducer to the object in cm.
        //    */
        //    var timeBetween = pulseLength.Elapsed;
        //    double? distance = timeBetween.TotalSeconds * 17000;

        //    // Check whether distance is out of range.
        //    if (timeBetween.TotalMilliseconds < 0.16 || timeBetween.TotalMilliseconds > 38)
        //        distance = null;

        //    Debug.WriteLine("Milliseconds: " + timeBetween.TotalMilliseconds);
        //    Debug.WriteLine("Distance: " + distance);

        //    return distance;
        //}

        public double GetDistance()
        {
            var mre = new ManualResetEventSlim(false);

            //Send a 10µs pulse to start the measurement
            triggerPin.Pin.Write(GpioPinValue.High);
            mre.Wait(TimeSpan.FromMilliseconds(0.01));
            triggerPin.Pin.Write(GpioPinValue.Low);

            var time = this.PulseIn(echoPin.Pin, GpioPinValue.High, 500);

            /* Calculating distance.
               If you take 340 m/sec (approximate speed of sound through air) and convert to cm/sec you get 34000 cm/sec.
               For pulse-echo, the sound travels twice the measured distance so you need to divide the conversion factor 
               by 2 so you get 17000 cm/sec. When you multiply by the measured time, you get distance from the transducer to the object in cm.
            */
            var distance = time * 17000;

            Debug.WriteLine("Milliseconds: " + time);
            Debug.WriteLine("Distance: " + distance);

            return distance;
        }

        protected override void OnTimer()
        {
            var distance = this.GetDistance();

            if (currentDistance != distance)
            {
                currentDistance = distance;
                RaiseEventHelper.CheckRaiseEventOnUIThread(this, DistanceChanged, RaiseEventsOnUIThread);
            }
        }

        private double PulseIn(GpioPin pin, GpioPinValue value, ushort timeout)
        {
            var sw = new Stopwatch();
            var swTimeout = new Stopwatch();

            swTimeout.Start();

            // Waits for pulse.
            while (pin.Read() != value)
            {
                if (swTimeout.ElapsedMilliseconds > timeout)
                    return 3.5;
            }

            sw.Start();

            // Waits for pulse end.
            while (pin.Read() == value)
            {
                if (swTimeout.ElapsedMilliseconds > timeout)
                    return 3.4;
            }

            sw.Stop();

            return sw.Elapsed.TotalSeconds;
        }

        public override void Dispose()
        {
            if (triggerPin != null)
                triggerPin.Dispose();

            if (echoPin != null)
                echoPin.Dispose();

            base.Dispose();
        }
    }
}