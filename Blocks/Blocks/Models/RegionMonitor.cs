#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class RegionMonitor
    {
        public int TotalChunkCount { get; internal set; }

        public int ActiveChunkCount { get; internal set; }

        public int TotalChunkMeshCount { get; internal set; }

        public int PassiveChunkMeshCount { get; internal set; }

        public int ActiveChunkMeshCount
        {
            get { return TotalChunkMeshCount - PassiveChunkMeshCount; }
        }

        public int TotalInterChunkMeshCount { get; internal set; }

        public int PassiveInterChunkMeshCount { get; internal set; }

        public int ActiveInterChunkMeshCount
        {
            get { return TotalInterChunkMeshCount - PassiveInterChunkMeshCount; }
        }

        public int TotalVertexBufferCount { get; internal set; }

        public int PassiveVertexBufferCount { get; internal set; }

        public int ActiveVertexBufferCount
        {
            get { return TotalVertexBufferCount - PassiveVertexBufferCount; }
        }

        public int VisibleOpaqueChunkCount { get; internal set; }

        public int VisibleTranslucentChunkCount { get; internal set; }

        public int OccludedOpaqueChunkCount { get; private set; }

        public void Clear()
        {
            TotalChunkCount = 0;
            ActiveChunkCount = 0;
            TotalChunkMeshCount = 0;
            PassiveChunkMeshCount = 0;
            TotalInterChunkMeshCount = 0;
            PassiveInterChunkMeshCount = 0;
            TotalVertexBufferCount = 0;
            PassiveVertexBufferCount = 0;
            VisibleOpaqueChunkCount = 0;
            VisibleTranslucentChunkCount = 0;
            OccludedOpaqueChunkCount = 0;
        }

        public void IncrementOccludedOpaqueChunkCount()
        {
            OccludedOpaqueChunkCount++;
        }
    }
}
