using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace IoTHelpers.Helpers
{
    internal static class RaiseEventHelper
    {
        public async static void CheckRaiseEventOnUIThread(object sender, EventHandler eventHandler, bool raiseEventsOnUIThread)
        {
            if (!raiseEventsOnUIThread)
            {
                eventHandler?.Invoke(sender, EventArgs.Empty);
            }
            else
            {
                var dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;
                if (dispatcher != null)
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => eventHandler?.Invoke(sender, EventArgs.Empty));
                else
                    eventHandler?.Invoke(sender, EventArgs.Empty);
            }
        }

        public async static void CheckRaiseEventOnUIThread<T>(object sender, EventHandler<T> eventHandler, T args, bool raiseEventsOnUIThread)
            where T : EventArgs
        {
            if (!raiseEventsOnUIThread)
            {
                eventHandler?.Invoke(sender, args);
            }
            else
            {
                var dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;
                if (dispatcher != null)
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => eventHandler?.Invoke(sender, args));
                else
                    eventHandler?.Invoke(sender, args);
            }
        }
    }
}
