#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public struct GraphicsSettingsDefinition
    {
        public bool ShadowMapEnabled;

        public ShadowMapSettingsDefinition ShadowMap;

        public bool SssmEnabled;

        public SssmSettingsDefinition Sssm;

        public bool SsaoEnabled;

        public SsaoSettingsDefinition Ssao;

        public bool EdgeEnabled;

        public EdgeSettingsDefinition Edge;

        public bool BloomEnabled;

        public BloomSettingsDefinition Bloom;

        public bool DofEnabled;

        public DofSettingsDefinition Dof;

        public bool ColorOverlapEnabled;

        public bool MonochromeEnabled;

        public bool LensFlareEnabled;
    }
}
