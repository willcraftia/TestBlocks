#if DEBUG

#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class RegionMonitor
    {
        public int TotalChunkCount { get; set; }

        public int ActiveChunkCount { get; set; }

        public int TotalChunkMeshCount { get; set; }

        public int PassiveChunkMeshCount { get; set; }

        public int ActiveChunkMeshCount
        {
            get { return TotalChunkMeshCount - PassiveChunkMeshCount; }
        }

        public int TotalInterChunkMeshCount { get; set; }

        public int PassiveInterChunkMeshCount { get; set; }

        public int ActiveInterChunkMeshCount
        {
            get { return TotalInterChunkMeshCount - PassiveInterChunkMeshCount; }
        }

        public int TotalVertexBufferCount { get; set; }

        public int PassiveVertexBufferCount { get; set; }

        public int ActiveVertexBufferCount
        {
            get { return TotalVertexBufferCount - PassiveVertexBufferCount; }
        }

        public int TotalChunkVertexCount { get; set; }

        public int TotalChunkIndexCount { get; set; }

        // ゲームを通しての最大を記録する。
        public int MaxChunkVertexCount { get; set; }

        // ゲームを通しての最大を記録する。
        public int MaxChunkIndexCount { get; set; }

        public int UpdatingChunkCount { get; set; }
    }
}

#endif