#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public interface ISceneModuleFactory
    {
        //====================================================================
        // 汎用

        Effect CreateGaussianBlurEffect();

        Effect CreateDepthMapEffect();

        Effect CreateNormalDepthMapEffect();

        //====================================================================
        // シャドウ マッピング

        Effect CreateShadowMapEffect();

        Effect CreateShadowSceneEffect();

        Effect CreateSssmEffect();

        //====================================================================
        // 被写界深度

        Effect CreateDofEffect();

        //====================================================================
        // エッジ強調

        Effect CreateEdgeEffect();

        //====================================================================
        // スクリーン スペース アンビエント オクルージョン

        Effect CreateSsaoMapEffect();

        Effect CreateSsaoMapBlurEffect();

        Effect CreateSsaoEffect();

        Texture2D CreateRandomNormalMap();

        //====================================================================
        // レンズ フレア

        Texture2D CreateLensFlareGlowSprite();

        Texture2D[] CreateLensFlareFlareSprites();
    }
}
