#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    /// <summary>
    /// ポイント スプライト パーティクルを描画するためのカスタム頂点構造体。
    /// </summary>
    struct ParticleVertex
    {
        // この頂点がパーティクルのどのコーナーに相当するかを保存します。
        public Short2 Corner;

        // パーティクルの開始位置を格納します。
        public Vector3 Position;

        // パーティクルの開始速度を格納します。
        public Vector3 Velocity;

        // 各パーティクルをわずかに異なる外観にするための 4 つのランダム値。
        public Color Random;

        // このパーティクルの作成時刻 (秒単位)。
        public float Time;

        // この頂点構造体のレイアウトを記述します。
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Short2, VertexElementUsage.Position, 0),
            new VertexElement(4, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
            new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(28, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(32, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0)
        );

        // この頂点構造体のサイズを記述します。
        public const int SizeInBytes = 36;
    }
}
