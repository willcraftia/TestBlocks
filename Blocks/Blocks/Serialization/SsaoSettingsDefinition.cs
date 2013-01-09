#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public struct SsaoSettingsDefinition
    {
        public bool Enabled;

        public float MapScale;

        public BlurSettingsDefinition Blur;
    }
}
