#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Diagnostics
{
    public struct LogEvent
    {
        public LogLevel Level;

        public string Category;

        public DateTime DateTime;

        public int ThreadId;

        public string Message;

        public Exception Exception;
    }
}
