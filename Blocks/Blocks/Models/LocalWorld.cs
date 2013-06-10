#region Using

using System;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class LocalWorld
    {
        public readonly IntVector3 Size;

        public IntVector3 Min;

        ChunkManager manager;

        Chunk[, ,] chunks;

        public Chunk this[int x, int y, int z]
        {
            get { return chunks[x, y, z]; }
        }

        public LocalWorld(ChunkManager manager, IntVector3 size)
        {
            if (manager == null) throw new ArgumentNullException("manager");
            if (size.X < 1 || size.Y < 1 || size.Z < 1) throw new ArgumentOutOfRangeException("size");

            this.manager = manager;
            Size = size;

            chunks = new Chunk[size.X, size.Y, size.Z];
        }

        public void FetchByCenter(IntVector3 center)
        {
            var min = new IntVector3
            {
                X = center.X - (Size.X - 1) / 2,
                Y = center.Y - (Size.Y - 1) / 2,
                Z = center.Z - (Size.Z - 1) / 2
            };

            FetchByMin(min);
        }

        public void FetchByMin(IntVector3 min)
        {
            Min = min;

            var position = new IntVector3();

            for (int z = 0; z < Size.Z; z++)
            {
                for (int y = 0; y < Size.Y; y++)
                {
                    for (int x = 0; x < Size.X; x++)
                    {
                        position.X = min.X + x;
                        position.Y = min.Y + y;
                        position.Z = min.Z + z;

                        chunks[x, y, z] = manager.GetChunk(ref position);
                    }
                }
            }
        }

        public bool GetChunkIndices(ref IntVector3 chunkPosition, out int x, out int y, out int z)
        {
            x = chunkPosition.X - Min.X;
            y = chunkPosition.Y - Min.Y;
            z = chunkPosition.Z - Min.Z;

            return 0 <= x || x < Size.X || 0 <= y || y < Size.Y || 0 <= z || z < Size.Z;
        }

        public Chunk GetChunk(ref IntVector3 chunkPosition)
        {
            var relative = chunkPosition - Min;

            if (relative.X < 0 || Size.X <= relative.X ||
                relative.Y < 0 || Size.Y <= relative.Y ||
                relative.Z < 0 || Size.Z <= relative.Z)
            {
                return null;
            }

            return chunks[relative.X, relative.Y, relative.Z];
        }

        public byte? GetBlockIndex(ref IntVector3 blockPosition)
        {
            IntVector3 chunkPosition;
            manager.GetChunkPositionByBlockPosition(ref blockPosition, out chunkPosition);

            var chunk = GetChunk(ref chunkPosition);
            if (chunk == null) return null;

            IntVector3 relativeBlockPosition;
            chunk.GetRelativeBlockPosition(ref blockPosition, out relativeBlockPosition);

            return chunk.GetBlockIndex(ref relativeBlockPosition);
        }

        public Block GetBlock(ref IntVector3 blockPosition)
        {
            IntVector3 chunkPosition;
            manager.GetChunkPositionByBlockPosition(ref blockPosition, out chunkPosition);

            var chunk = GetChunk(ref chunkPosition);
            if (chunk == null) return null;

            IntVector3 relativeBlockPosition;
            chunk.GetRelativeBlockPosition(ref blockPosition, out relativeBlockPosition);

            var blockIndex = chunk.GetBlockIndex(ref relativeBlockPosition);
            if (blockIndex == Block.EmptyIndex) return null;

            return chunk.Region.BlockCatalog[blockIndex];
        }

        public byte GetSkylightLevel(ref IntVector3 blockPosition)
        {
            IntVector3 chunkPosition;
            manager.GetChunkPositionByBlockPosition(ref blockPosition, out chunkPosition);

            var chunk = GetChunk(ref chunkPosition);
            if (chunk == null) return Chunk.MaxSkylightLevel;

            IntVector3 relativeBlockPosition;
            chunk.GetRelativeBlockPosition(ref blockPosition, out relativeBlockPosition);

            return chunk.GetSkylightLevel(ref relativeBlockPosition);
        }

        public void SetSkylightLevel(ref IntVector3 blockPosition, byte value)
        {
            IntVector3 chunkPosition;
            manager.GetChunkPositionByBlockPosition(ref blockPosition, out chunkPosition);

            var chunk = GetChunk(ref chunkPosition);
            if (chunk == null) return;

            IntVector3 relativeBlockPosition;
            chunk.GetRelativeBlockPosition(ref blockPosition, out relativeBlockPosition);

            chunk.SetSkylightLevel(ref relativeBlockPosition, value);
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
