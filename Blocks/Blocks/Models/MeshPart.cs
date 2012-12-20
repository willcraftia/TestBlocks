#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class MeshPart
    {
        public VertexPositionNormalTexture[] Vertices { get; private set; }

        public ushort[] Indices { get; private set; }

        public MeshPart(VertexPositionNormalTexture[] vertices, ushort[] indices)
        {
            if (vertices == null) throw new ArgumentNullException("vertices");
            if (indices == null) throw new ArgumentNullException("indices");

            Vertices = vertices;
            Indices = indices;
        }

        public MeshPart Create(ref Vector2 texCoordOffset)
        {
            var newVertices = new VertexPositionNormalTexture[Vertices.Length];
            Array.Copy(Vertices, newVertices, newVertices.Length);
            for (int j = 0; j < newVertices.Length; j++)
            {
                newVertices[j].TextureCoordinate.X += texCoordOffset.X;
                newVertices[j].TextureCoordinate.Y += texCoordOffset.Y;
            }

            // 全てのメッシュで共通であるため配列を共有。
            return new MeshPart(newVertices, Indices);
        }
    }
}
