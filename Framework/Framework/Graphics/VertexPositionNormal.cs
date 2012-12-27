#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    /// <summary>
    /// 頂点の位置と法線を保持するカスタム頂点構造体です。
    /// </summary>
    public struct VertexPositionNormal : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0));

        /// <summary>
        /// 頂点の位置。
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// 頂点の法線。
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="position">頂点の位置。</param>
        /// <param name="normal">頂点の法線。</param>
        public VertexPositionNormal(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }

        // I/F
        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }
}
