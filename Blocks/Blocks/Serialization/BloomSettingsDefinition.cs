#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public struct BloomSettingsDefinition
    {
        public bool Enabled;

        public float MapScale;

        public BlurSettingsDefinition Blur;
    }
}
