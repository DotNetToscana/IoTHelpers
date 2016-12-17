using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTHelpers.Boards
{
    public static class ShutdownManager
    {
        public static void Shutdown() => Shutdown(TimeSpan.FromSeconds(0));

        public static void Shutdown(TimeSpan timeout) => PerformShutdown(Windows.System.ShutdownKind.Shutdown, timeout);

        public static void Restart() => Restart(TimeSpan.FromSeconds(0));

        public static void Restart(TimeSpan timeout) => PerformShutdown(Windows.System.ShutdownKind.Restart, timeout);

        private static void PerformShutdown(Windows.System.ShutdownKind kind, TimeSpan timeout)
            => Task.Run(() => Windows.System.ShutdownManager.BeginShutdown(kind, timeout));
    }
}
