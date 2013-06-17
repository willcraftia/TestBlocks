#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class SetBlockCommand : WorldCommand
    {
        public IntVector3 BlockPosition;

        public byte BlockIndex;

        byte lastBlockIndex;

        internal SetBlockCommand() { }

        public override bool Do()
        {
            SetBlock(BlockIndex);
            return true;
        }

        public override void Undo()
        {
            SetBlock(lastBlockIndex);
        }

        void SetBlock(byte blockIndex)
        {
            var chunkManager = WorldManager.ChunkManager;

            var chunk = chunkManager.GetChunkByBlockPosition(BlockPosition);
            if (chunk == null) throw new InvalidOperationException("Chunk not found: BlockPosition=" + BlockPosition);

            var relativePosition = chunk.GetRelativeBlockPosition(BlockPosition);

            lastBlockIndex = chunk.GetBlockIndex(relativePosition);

            // 既存ブロックと同じならば処理せず、Undo 履歴にも残さない。
            if (blockIndex == lastBlockIndex) return;

            chunk.SetBlockIndex(relativePosition, blockIndex);

            var lightUpdater = chunkManager.BorrowLightUpdater();

            if (blockIndex == Block.EmptyIndex)
            {
                lightUpdater.UpdateSkylightLevelByBlockRemoved(ref BlockPosition);
            }
            else
            {
                lightUpdater.UpdateSkylightLevelByBlockCreated(ref BlockPosition);
            }

            for (int i = 0; i < lightUpdater.AffectedChunks.Count; i++)
            {
                var affectedChunk = lightUpdater.AffectedChunks[i];
                chunkManager.RequestBuildVertices(affectedChunk, ChunkMeshUpdatePriority.High);
            }

            chunkManager.ReturnLightUpdater(lightUpdater);
        }
    }
}
