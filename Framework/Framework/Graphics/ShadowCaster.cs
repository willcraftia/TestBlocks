﻿#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public abstract class ShadowCaster : SceneObject
    {
        public bool CastShadow { get; set; }

        protected ShadowCaster()
        {
            CastShadow = true;
        }

        // シャドウ マップ/シャドウ シーンへの描画。
        public abstract void Draw(IEffectShadow effect);
    }
}