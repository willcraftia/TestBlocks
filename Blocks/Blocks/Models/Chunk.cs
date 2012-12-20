#region Using

using System;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class Chunk
    {
        VectorI3 size;

        VectorI3 position;

        BoundingBox boundingBox;

        byte[] blockIndices;

        public VectorI3 Size
        {
            get { return size; }
        }

        public VectorI3 Position
        {
            get { return position; }
            set
            {
                position = value;

                boundingBox.Min = new Vector3
                {
                    X = position.X * size.X,
                    Y = position.Y * size.Y,
                    Z = position.Z * size.Z
                };
                boundingBox.Max = new Vector3
                {
                    X = boundingBox.Min.X + size.X,
                    Y = boundingBox.Min.Y + size.Y,
                    Z = boundingBox.Min.Z + size.Z
                };
            }
        }

        public BoundingBox BoundingBox
        {
            get { return boundingBox; }
        }

        public byte this[int x, int y, int z]
        {
            get
            {
                if (x < 0 || size.X < x) throw new ArgumentOutOfRangeException("x");
                if (y < 0 || size.Y < y) throw new ArgumentOutOfRangeException("y");
                if (z < 0 || size.Z < z) throw new ArgumentOutOfRangeException("z");

                var index = (x * size.X * size.Y) + (y * size.X) + z;
                return blockIndices[index];
            }
            set
            {
                if (x < 0 || size.X < x) throw new ArgumentOutOfRangeException("x");
                if (y < 0 || size.Y < y) throw new ArgumentOutOfRangeException("y");
                if (z < 0 || size.Z < z) throw new ArgumentOutOfRangeException("z");

                var index = (x * size.X * size.Y) + (y * size.X) + z;
                blockIndices[index] = value;
            }
        }

        public byte this[int index]
        {
            get { return blockIndices[index]; }
            set { blockIndices[index] = value; }
        }

        public int Count
        {
            get { return blockIndices.Length; }
        }

        public bool Dirty { get; set; }

        public ChunkMesh ActiveMesh { get; set; }

        public ChunkMesh PendingMesh { get; set; }

        public Chunk(VectorI3 size)
        {
            this.size = size;

            blockIndices = new byte[size.X * size.Y * size.Z];
        }

        public int CalculateBlockPositionX(int x)
        {
            return position.X * size.X + x;
        }

        public int CalculateBlockPositionY(int y)
        {
            return position.Y * size.Y + y;
        }

        public int CalculateBlockPositionZ(int z)
        {
            return position.Z * size.Z + z;
        }

        public void Read(BinaryReader reader)
        {
            position.X = reader.ReadInt32();
            position.Y = reader.ReadInt32();
            position.Z = reader.ReadInt32();

            for (int i = 0; i < blockIndices.Length; i++)
                blockIndices[i] = reader.ReadByte();

            Dirty = true;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(position.X);
            writer.Write(position.Y);
            writer.Write(position.Z);

            for (int i = 0; i < blockIndices.Length; i++)
                writer.Write(blockIndices[i]);
        }

        public void Clear()
        {
            Array.Clear(blockIndices, 0, blockIndices.Length);
            Dirty = true;
        }

        public bool Contains(ref VectorI3 blockPosition)
        {
            return 0 <= blockPosition.X && blockPosition.X < size.X &&
                0 <= blockPosition.Y && blockPosition.Y < size.Y &&
                0 <= blockPosition.Z && blockPosition.Z < size.Z;
        }
    }
}
