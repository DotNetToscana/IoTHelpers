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
        private readonly int timerPeriod;

        private ReadingMode mode;
        public ReadingMode Mode
        {
            get { return mode; }
            set { this.Initialize(value); }
        }

        public TimedModule(ReadingMode mode, int timerPeriod)
        {
            this.timerPeriod = timerPeriod;
            this.mode = mode;

            timer = new Timer(CheckState, null, Timeout.Infinite, Timeout.Infinite);
        }

        protected void Initialize() => this.Initialize(mode);

        private void Initialize(ReadingMode newMode)
        {
            this.mode = newMode;            

            if (mode == ReadingMode.Continuous)
                timer.Change(0, timerPeriod);
            else
                timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void CheckState(object state) => OnTimer();

        protected abstract void OnTimer();

        public virtual void Dispose()
        {
            timer.Dispose();
        }
    }

    public abstract class GpioTimedModuleBase : TimedModule
    {
        protected GpioController Controller { get; }

        public GpioTimedModuleBase(ReadingMode mode, int timerPeriod)
            : this(GpioController.GetDefault(), mode, timerPeriod)
        { }

        protected GpioTimedModuleBase(GpioController controller, ReadingMode mode, int timerPeriod)
            : base(mode, timerPeriod)
        {
            Controller = controller;

            // Shows an error if there is no GPIO controller
            if (Controller == null)
                throw new ArgumentException("No GPIO controller found.");
        }
    }
}
