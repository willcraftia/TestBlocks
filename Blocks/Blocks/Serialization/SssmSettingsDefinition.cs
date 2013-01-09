#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public struct SssmSettingsDefinition
    {
        public float MapScale;

        public bool BlurEnabled;

        public BlurSettingsDefinition Blur;
    }
}
