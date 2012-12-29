#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public interface ISceneObject
    {
        ISceneObjectContext Context { set; }

        bool Visible { get; }

        bool Translucent { get; }

        bool Occluded { get; }

        // 視点からの距離によるソートで用いる。
        void GetDistanceSquared(ref Vector3 eyePosition, out float result);

        void GetBoundingSphere(out BoundingSphere result);

        void GetBoundingBox(out BoundingBox result);

        void UpdateOcclusion();

        // シーン マップへの描画。
        // シャドウを用いない場合は null 指定の想定。
        // 実装では、null か否かでエフェクトのテクニックを変更するなど。
        void Draw(Texture2D shadowMap);
    }
}
