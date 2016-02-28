using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTHelpers.Gpio
{
    public abstract class TimedModule : IDisposable
    {
        private readonly Timer timer;

        private ReadingMode mode;
        public ReadingMode Mode
        {
            get { return mode; }
            set { this.Initialize(value); }
        }

        private TimeSpan readInterval;
        public TimeSpan ReadInterval
        {
            get { return readInterval; }
            set
            {
                if (Mode == ReadingMode.Manual)
                    throw new NotSupportedException($"You cannot change {nameof(ReadInterval)} when {nameof(Mode)} is set to {ReadingMode.Manual}.");

                readInterval = value;
                timer?.Change(0, (int)readInterval.TotalMilliseconds);
            }
        }

        public TimedModule(ReadingMode mode, TimeSpan readInterval)
        {
            this.readInterval = readInterval;
            this.mode = mode;

            timer = new Timer(CheckState, null, Timeout.Infinite, Timeout.Infinite);
        }

        protected void InitializeTimer() => this.Initialize(mode);

        private void Initialize(ReadingMode newMode)
        {
            this.mode = newMode;

            if (mode == ReadingMode.Continuous)
                timer.Change(0, (int)readInterval.TotalMilliseconds);
            else
                timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void CheckState(object state) => OnTimer();

        protected abstract void OnTimer();

        public virtual void Dispose()
        {
            timer.Dispose();
        }
    }

    public abstract class GpioTimedModuleBase : TimedModule
    {
        protected GpioController Controller { get; }

        public GpioTimedModuleBase(ReadingMode mode, TimeSpan readInterval)
            : this(GpioController.GetDefault(), mode, readInterval)
        { }

        protected GpioTimedModuleBase(GpioController controller, ReadingMode mode, TimeSpan readInterval)
            : base(mode, readInterval)
        {
            Controller = controller;

            // Shows an error if there is no GPIO controller
            if (Controller == null)
                throw new ArgumentException("No GPIO controller found.");
        }
    }
}
