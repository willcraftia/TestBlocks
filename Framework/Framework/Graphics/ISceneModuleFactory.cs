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

        //====================================================================
        // シャドウ マッピング

        Effect CreateShadowMapEffect();
    }
}
