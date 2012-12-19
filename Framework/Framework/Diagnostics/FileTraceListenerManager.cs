#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

#endregion

namespace Willcraftia.Xna.Framework.Diagnostics
{
    public static class FileTraceListenerManager
    {
#if WINDOWS
        static List<TextWriterTraceListener> fileTraceListeners;
#endif

        [Conditional("TRACE"), Conditional("DEBUG"), Conditional("WINDOWS")]
        public static void Add(string filePath, bool append)
        {
#if WINDOWS
            var fullPath = Path.GetFullPath(filePath);

            var directoryPath = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            var writer = new StreamWriter(fullPath, append);
            var listener = new TextWriterTraceListener(writer);
            listener.Name = filePath;
            Trace.Listeners.Add(listener);

            if (fileTraceListeners == null) fileTraceListeners = new List<TextWriterTraceListener>();
            fileTraceListeners.Add(listener);
#endif
        }

        [Conditional("TRACE"), Conditional("DEBUG"), Conditional("WINDOWS")]
        public static void Clear()
        {
#if WINDOWS
            if (fileTraceListeners == null) return;

            foreach (var listener in fileTraceListeners)
            {
                listener.Flush();
                listener.Close();
                Trace.Listeners.Remove(listener);
            }

            fileTraceListeners.Clear();
#endif
        }
    }
}
