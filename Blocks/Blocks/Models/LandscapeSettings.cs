#region Using

using System;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Landscape;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class LandscapeSettings : IAsset
    {
        int minActiveRange = 10;

        int maxActiveRange = 12;

        // I/F
        public IResource Resource { get; set; }

        public int MinActiveRange
        {
            get { return minActiveRange; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                minActiveRange = value;
            }
        }

        public int MaxActiveRange
        {
            get { return maxActiveRange; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                maxActiveRange = value;
            }
        }

        public PartitionManager.Settings PartitionManager { get; private set; }

        public LandscapeSettings()
        {
            PartitionManager = new PartitionManager.Settings();
        }
    }
}
