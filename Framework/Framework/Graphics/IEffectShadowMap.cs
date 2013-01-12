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

        int ShadowMapSize { get; set; }

        ShadowMap.Techniques ShadowMapTechnique { get; set; }

        float ShadowMapDepthBias { get; set; }

        int ShadowMapCount { get; set; }

        float[] ShadowMapDistances { get; set; }

        Matrix[] ShadowMapLightViewProjections { get; set; }

        Texture2D[] ShadowMaps { get; set; }
    }
}
