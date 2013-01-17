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

        public bool GetChunk(VectorI3 position, ChunkData data) { return false; }

        public void AddChunk(VectorI3 position, ChunkData data) { }

        public void DeleteChunk(VectorI3 position) { }

        public void ClearChunks() { }

        public void GetChunkBundle(Stream chunkBundleStream) { }

        public void AddChunkBundle(Stream chunkBundleStream) { }
    }
}
