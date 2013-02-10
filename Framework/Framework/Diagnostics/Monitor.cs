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
            for (int i = 0; i < Listeners.Count; i++)
                Listeners[i].Begin(name);
        }

        [Conditional("DEBUG"), Conditional("TRACE")]
        public static void End(string name)
        {
            for (int i = 0; i < Listeners.Count; i++)
                Listeners[i].End(name);
        }
    }
}
