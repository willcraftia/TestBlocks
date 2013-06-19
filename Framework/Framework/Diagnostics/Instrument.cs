#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Diagnostics
{
    public sealed class Instrument
    {
        static readonly Stack<string> stack = new Stack<string>(4);

        public static List<IInstrumentListener> Listeners { get; private set; }

        static Instrument()
        {
            Listeners = new List<IInstrumentListener>();
        }

        [Conditional("DEBUG"), Conditional("TRACE")]
        public static void Begin(string name)
        {
            stack.Push(name);

            for (int i = 0; i < Listeners.Count; i++)
                Listeners[i].Begin(name);
        }

        [Conditional("DEBUG"), Conditional("TRACE")]
        public static void End()
        {
            var name = stack.Pop();

            for (int i = 0; i < Listeners.Count; i++)
                Listeners[i].End(name);
        }
    }
}
