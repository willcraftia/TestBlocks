#region Using

using System;
using System.IO;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public interface IChunkStore
    {
        bool GetChunk(ref VectorI3 position, Chunk chunk);

        void AddChunk(Chunk chunk);

        void DeleteChunk(Chunk chunk);

        void ClearChunks();

        //
        // ChunkBundle の反映は、エディタとゲームで共通。
        // ChunkBundle の取得は、エディタのため。
        //

        void GetChunkBundle(Stream chunkBundleStream, ref VectorI3 chunkSize);

        void AddChunkBundle(Stream chunkBundleStream, ref VectorI3 chunkSize);
    }
}
