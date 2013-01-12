#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public interface IEffectShadowMap
    {
        bool ShadowMapEnabled { get; set; }

        ShadowMap.Techniques ShadowMapTechnique { get; set; }

        float DepthBias { get; set; }

        int SplitCount { get; set; }

        float[] SplitDistances { get; set; }

        Matrix[] SplitLightViewProjections { get; set; }

        Texture2D[] SplitShadowMaps { get; set; }
    }
}
