#region Using

using System;
using System.IO;
using System.Threading;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class Chunk
    {
        VectorI3 size;

        byte[] blockIndices;

        //====================================================================
        // Efficiency

        public VectorI3 Position;

        //
        //====================================================================

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

        public void Read(BinaryReader reader)
        {
            Position.X = reader.ReadInt32();
            Position.Y = reader.ReadInt32();
            Position.Z = reader.ReadInt32();

            for (int i = 0; i < blockIndices.Length; i++)
                blockIndices[i] = reader.ReadByte();

            Dirty = true;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(Position.Z);

            for (int i = 0; i < blockIndices.Length; i++)
                writer.Write(blockIndices[i]);
        }

        public void Clear()
        {
            Array.Clear(blockIndices, 0, blockIndices.Length);
            Dirty = true;
        }

        public bool Contains(ref VectorI3 position)
        {
            return 0 <= position.X && position.X < size.X &&
                0 <= position.Y && position.Y < size.Y &&
                0 <= position.Z && position.Z < size.Z;
        }
    }
}
