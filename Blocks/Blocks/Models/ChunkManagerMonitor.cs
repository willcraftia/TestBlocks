#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkManagerMonitor
    {
        public int TotalChunkCount { get; set; }

        public int ActiveChunkCount { get; set; }

        public int TotalChunkMeshCount { get; set; }

        public int PassiveChunkMeshCount { get; set; }

        public int ActiveChunkMeshCount
        {
            get { return TotalChunkMeshCount - PassiveChunkMeshCount; }
        }

        public int TotalInterChunkCount { get; set; }

        public int PassiveInterChunkCount { get; set; }

        public int ActiveInterChunkCount
        {
            get { return TotalInterChunkCount - PassiveInterChunkCount; }
        }

        public int TotalBufferCount { get; set; }

        public int PassiveBufferCount { get; set; }

        public int ActiveBufferCount
        {
            get { return TotalBufferCount - PassiveBufferCount; }
        }

        public int TotalVertexCount { get; set; }

        public int TotalIndexCount { get; set; }

        public int VertexCapacity { get; set; }

        public int IndexCapacity { get; set; }

        public int AllocatedVertexCount
        {
            get { return TotalBufferCount * VertexCapacity; }
        }

        public int AllocatedIndexCount
        {
            get { return TotalBufferCount * IndexCapacity; }
        }

        // ゲームを通しての最大を記録する。
        public int MaxVertexCount { get; set; }

        // ゲームを通しての最大を記録する。
        public int MaxIndexCount { get; set; }

        public int UpdatingChunkCount { get; set; }
    }
}
