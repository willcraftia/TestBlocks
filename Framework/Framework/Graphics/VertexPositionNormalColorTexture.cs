#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public struct VertexPositionNormalColorTexture : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));

        /// <summary>
        /// 頂点の位置。
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// 頂点の法線。
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// 頂点の色。
        /// </summary>
        public Color Color;

        /// <summary>
        /// 頂点のテクスチャ座標。
        /// </summary>
        public Vector2 TextureCoordinate;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="position">頂点の位置。</param>
        /// <param name="normal">頂点の法線。</param>
        /// <param name="color">頂点の色。</param>
        /// <param name="textureCoordinate">頂点のテクスチャ座標。</param>
        public VertexPositionNormalColorTexture(Vector3 position, Vector3 normal, Color color, Vector2 textureCoordinate)
        {
            Position = position;
            Normal = normal;
            Color = color;
            TextureCoordinate = textureCoordinate;
        }

        // I/F
        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }
}
