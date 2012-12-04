#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Landscape
{
    public sealed class ChunkRenderer
    {
        public Matrix View;

        public Matrix Projection;

        Queue<ChunkPartition> opaqueChunkQueue = new Queue<ChunkPartition>();

        public void Draw()
        {
            while (0 < opaqueChunkQueue.Count)
            {
                var chunk = opaqueChunkQueue.Dequeue();
                DrawOpaqueChunk(chunk);
            }
        }

        void DrawOpaqueChunk(ChunkPartition chunk)
        {
            if (!chunk.IsActivationCompleted) return;

            //chunk.DrawOpaqueMesh();
        }
    }
}
