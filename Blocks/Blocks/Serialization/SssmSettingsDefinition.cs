#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public struct SssmSettingsDefinition
    {
        public bool Enabled;

        public bool BlurEnabled;

        public BlurSettingsDefinition Blur;

        public float MapScale;
    }
}
