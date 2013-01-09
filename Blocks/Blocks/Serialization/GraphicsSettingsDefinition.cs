#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public struct GraphicsSettingsDefinition
    {
        public ShadowMapSettingsDefinition ShadowMap;

        public SssmSettingsDefinition Sssm;

        public SsaoSettingsDefinition Ssao;

        public EdgeSettingsDefinition Edge;

        public BloomSettingsDefinition Bloom;

        public DofSettingsDefinition Dof;
    }
}
