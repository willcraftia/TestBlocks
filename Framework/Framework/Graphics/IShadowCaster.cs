#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public interface IShadowCaster : ISceneObject
    {
        bool CastShadow { get; }

        // シャドウ マップ/シャドウ シーンへの描画。
        void Draw(IEffectShadow effect);
    }
}
