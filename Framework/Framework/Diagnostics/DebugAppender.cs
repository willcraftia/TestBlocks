#region Using

using System;
using System.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Diagnostics
{
    public sealed class DebugAppender : IAppender
    {
        public static DebugAppender Instance = new DebugAppender();

        static readonly string[] levelStrings =
        {
            "FATAL", "ERROR", "WARN ", "INFO ", "DEBUG"
        };

        DebugAppender() { }

        public void Append(ref LogEvent logEvent)
        {
            Debug.WriteLine("{0:HH:mm:ss.fffffff} [{1}] {2} {3} - {4}",
                logEvent.DateTime,
                logEvent.ThreadId,
                levelStrings[(int) logEvent.Level],
                logEvent.Category,
                logEvent.Message);

            if (logEvent.Exception != null) AppendException(logEvent.Exception);
        }

        void AppendException(Exception exception)
        {
            Debug.WriteLine("{0}: {1}", exception.GetType().FullName, exception.Message);
            Debug.WriteLine(exception.StackTrace);

            if (exception.InnerException != null) AppendException(exception.InnerException);
        }
    }
}
