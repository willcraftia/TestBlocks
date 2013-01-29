#region Using

using System;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class LocalWorld
    {
        public readonly VectorI3 Size;

        ChunkManager manager;

        Chunk[, ,] chunks;

        public Chunk this[int x, int y, int z]
        {
            get { return chunks[x, y, z]; }
        }

        public LocalWorld(ChunkManager manager, VectorI3 size)
        {
            if (manager == null) throw new ArgumentNullException("manager");
            if (size.X < 1 || size.Y < 1 || size.Z < 1) throw new ArgumentOutOfRangeException("size");

            this.manager = manager;
            Size = size;

            chunks = new Chunk[size.X, size.Y, size.Z];
        }

        public void FetchByCenter(VectorI3 center)
        {
            var min = new VectorI3
            {
                X = center.X - (Size.X - 1) / 2,
                Y = center.Y - (Size.Y - 1) / 2,
                Z = center.Z - (Size.Z - 1) / 2
            };

            FetchByMin(min);
        }

        public void FetchByMin(VectorI3 min)
        {
            var position = new VectorI3();

            for (int z = 0; z < Size.Z; z++)
            {
                for (int y = 0; y < Size.Y; y++)
                {
                    for (int x = 0; x < Size.X; x++)
                    {
                        position.X = min.X + x;
                        position.Y = min.Y + y;
                        position.Z = min.Z + z;

                        chunks[x, y, z] = manager[position] as Chunk;
                    }
                }
            }
        }

        /// <summary>
        /// 状態を初期化します。
        /// </summary>
        public void Clear()
        {
            Array.Clear(chunks, 0, chunks.Length);
        }
    }
}
