#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Diagnostics
{
    public sealed class Monitor
    {
        public static List<IMonitorListener> Listeners { get; private set; }

        static Monitor()
        {
            Listeners = new List<IMonitorListener>();
        }

        [Conditional("DEBUG"), Conditional("TRACE")]
        public static void Begin(string name)
        {
            foreach (var listener in Listeners)
                listener.Begin(name);
        }

        [Conditional("DEBUG"), Conditional("TRACE")]
        public static void End(string name)
        {
            foreach (var listener in Listeners)
                listener.End(name);
        }
    }
}
