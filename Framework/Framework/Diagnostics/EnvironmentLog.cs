#region Using

using System;
using System.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Diagnostics
{
    public static class EnvironmentLog
    {
        public static readonly Logger Logger = new Logger(typeof(Environment).Name);

        [Conditional("TRACE")]
        public static void Info()
        {
            Logger.Info("OSVersion:      {0}", Environment.OSVersion);
            Logger.Info("ProcessorCount: {0}", Environment.ProcessorCount);
            Logger.Info("Version:        {0}", Environment.Version);
        }
    }
}
