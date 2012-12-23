#if DEBUG

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

        public int TotalChunkVertexCount { get; private set; }

        public int TotalChunkIndexCount { get; private set; }

        // ゲームを通しての最大を記録する。
        public int MaxChunkVertexCount { get; private set; }

        // ゲームを通しての最大を記録する。
        public int MaxChunkIndexCount { get; private set; }

        public int UpdatingChunkCount { get; internal set; }

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
            TotalChunkVertexCount = 0;
            TotalChunkIndexCount = 0;
            UpdatingChunkCount = 0;
        }

        public void IncrementOccludedOpaqueChunkCount()
        {
            OccludedOpaqueChunkCount++;
        }

        public void AddChunkVertexCount(int vertexCount)
        {
            TotalChunkVertexCount += vertexCount;
            MaxChunkVertexCount = Math.Max(MaxChunkVertexCount, vertexCount);
        }

        public void AddChunkIndexCount(int indexCount)
        {
            TotalChunkIndexCount += indexCount;
            MaxChunkIndexCount = Math.Max(MaxChunkIndexCount, indexCount);
        }
    }
}

#endif