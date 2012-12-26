#region Using

using System;
using System.IO;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class NullChunkStore : IChunkStore
    {
        public static readonly NullChunkStore Instance = new NullChunkStore();

        NullChunkStore() { }

        public bool GetChunk(ref VectorI3 position, Chunk chunk)
        {
            return false;
        }

        public void AddChunk(Chunk chunk)
        {
        }

        public void DeleteChunk(Chunk chunk)
        {
        }

        public void ClearChunks()
        {
        }

        public void GetChunkBundle(Stream chunkBundleStream, ref VectorI3 chunkSize)
        {
        }

        public void AddChunkBundle(Stream chunkBundleStream, ref VectorI3 chunkSize)
        {
        }
    }
}
