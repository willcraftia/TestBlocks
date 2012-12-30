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

        // シャドウ マップへの描画。
        void DrawShadow(ShadowMapEffect effect);
    }
}
