#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public sealed class PartitionManagerMonitor
    {
        public int ActiveClusterCount { get; set; }

        public int ActivePartitionCount { get; set; }

        public int ActivatingPartitionCount { get; set; }

        public int PassivatingPartitionCount { get; set; }
    }
}
