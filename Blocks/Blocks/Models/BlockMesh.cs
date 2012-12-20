#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        public MeshPart this[Side side]
        {
            get
            {
                switch (side)
                {
                    case Side.Top:
                        return Top;
                    case Side.Bottom:
                        return Bottom;
                    case Side.Front:
                        return Front;
                    case Side.Back:
                        return Back;
                    case Side.Left:
                        return Left;
                    case Side.Right:
                        return Right;
                    default:
                        throw new InvalidOperationException();
                }
            }
            private set
            {
                switch (side)
                {
                    case Side.Top:
                        Top = value;
                        break;
                    case Side.Bottom:
                        Bottom = value;
                        break;
                    case Side.Front:
                        Front = value;
                        break;
                    case Side.Back:
                        Back = value;
                        break;
                    case Side.Left:
                        Left = value;
                        break;
                    case Side.Right:
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
                var side = (Side) i;

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
