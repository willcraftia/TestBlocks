#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class BlockMesh
    {
        public MeshPart Top { get; private set; }

        public MeshPart Bottom { get; private set; }

        public MeshPart Front { get; private set; }

        public MeshPart Back { get; private set; }

        public MeshPart Left { get; private set; }

        public MeshPart Right { get; private set; }

        public MeshPart this[CubeSides side]
        {
            get
            {
                switch (side)
                {
                    case CubeSides.Top:
                        return Top;
                    case CubeSides.Bottom:
                        return Bottom;
                    case CubeSides.Front:
                        return Front;
                    case CubeSides.Back:
                        return Back;
                    case CubeSides.Left:
                        return Left;
                    case CubeSides.Right:
                        return Right;
                    default:
                        throw new InvalidOperationException();
                }
            }
            private set
            {
                switch (side)
                {
                    case CubeSides.Top:
                        Top = value;
                        break;
                    case CubeSides.Bottom:
                        Bottom = value;
                        break;
                    case CubeSides.Front:
                        Front = value;
                        break;
                    case CubeSides.Back:
                        Back = value;
                        break;
                    case CubeSides.Left:
                        Left = value;
                        break;
                    case CubeSides.Right:
                        Right = value;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        BlockMesh() { }

        public static BlockMesh Create(Block block)
        {
            var mesh = new BlockMesh();

            for (int i = 0; i < 6; i++)
            {
                var side = (CubeSides) i;

                var prototype = block.MeshPrototype[side];
                if (prototype == null) continue;

                var texCoordOffset = Vector2.Zero;

                var tile = block.GetTile(side);
                if (tile != null) tile.GetTexCoordOffset(out texCoordOffset);

                mesh[side] = Create(prototype, ref texCoordOffset);
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
