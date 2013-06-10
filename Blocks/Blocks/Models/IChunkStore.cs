#region Using

using System;
using System.IO;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public interface IChunkStore
    {
        bool GetChunk(string regionKey, IntVector3 position, ChunkData data);

        void AddChunk(string regionKey, IntVector3 position, ChunkData data);

        void DeleteChunk(string regionKey, IntVector3 position);

        void ClearChunks(string regionKey);

        //
        // ChunkBundle の反映は、エディタとゲームで共通。
        // ChunkBundle の取得は、エディタのため。
        //

        void GetChunkBundle(Stream chunkBundleStream);

        void AddChunkBundle(Stream chunkBundleStream);
    }
}
