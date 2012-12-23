#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class BlockMesh
    {
        public CubicCollection<MeshPart> MeshParts { get; private set; }

        BlockMesh()
        {
            MeshParts = new CubicCollection<MeshPart>();
        }

        public static BlockMesh Create(Block block)
        {
            var mesh = new BlockMesh();

            foreach (var side in CubicSide.Items)
            {
                var prototype = block.MeshPrototype.MeshParts[side];
                if (prototype == null) continue;

                var texCoordOffset = Vector2.Zero;

                var tile = block.Tiles[side];
                if (tile != null) tile.GetTexCoordOffset(out texCoordOffset);

                mesh.MeshParts[side] = Create(prototype, ref texCoordOffset);
            }

            return mesh;
        }

        static MeshPart Create(MeshPart prototype, ref Vector2 texCoordOffset)
        {
            var newVertices = new VertexPositionNormalTexture[prototype.Vertices.Length];
            Array.Copy(prototype.Vertices, newVertices, newVertices.Length);

            for (int j = 0; j < newVertices.Length; j++)
            {
                newVertices[j].TextureCoordinate.X *= Tile.InverseSize;
                newVertices[j].TextureCoordinate.Y *= Tile.InverseSize;
                newVertices[j].TextureCoordinate.X += texCoordOffset.X;
                newVertices[j].TextureCoordinate.Y += texCoordOffset.Y;
            }

            // 全てのメッシュで共通であるため配列を共有。
            return new MeshPart(newVertices, prototype.Indices);
        }
    }
}
