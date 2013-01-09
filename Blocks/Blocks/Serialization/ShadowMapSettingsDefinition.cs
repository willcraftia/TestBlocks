#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public struct ShadowMapSettingsDefinition
    {
        public ShadowMap.Techniques Technique;

        public int Size;

        public float DepthBias;

        public float FarPlaneDistance;

        public int SplitCount;

        public float SplitLambda;

        public BlurSettingsDefinition VsmBlur;
    }
}
