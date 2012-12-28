#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public interface IPssmSceneEffect : IEffectMatrices
    {
        float DepthBias { get; set; }

        int SplitCount { get; set; }

        float[] SplitDistances { get; set; }

        Matrix[] SplitViewProjections { get; set; }

        void SetShadowMap(int index, Texture2D shadowMap);
    }
}
