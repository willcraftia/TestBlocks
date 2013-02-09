#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class SetBlockCommand : WorldCommand
    {
        public VectorI3 BlockPosition;

        public byte BlockIndex;

        byte lastBlockIndex;

        internal SetBlockCommand() { }

        public override bool Do()
        {
            SetAndUpdateBlock(BlockIndex);
            return true;
        }

        public override void Undo()
        {
            SetAndUpdateBlock(lastBlockIndex);
        }

        void SetAndUpdateBlock(byte blockIndex)
        {
            var chunkManager = WorldManager.ChunkManager;

            var chunk = chunkManager.GetChunkByBlockPosition(ref BlockPosition);
            if (chunk == null) throw new InvalidOperationException("Chunk not found: BlockPosition=" + BlockPosition);

            VectorI3 relativePosition;
            chunk.GetRelativeBlockPosition(ref BlockPosition, out relativePosition);

            lastBlockIndex = chunk.GetBlockIndex(ref relativePosition);

            chunk.SetBlockIndex(ref relativePosition, blockIndex);

            var lightUpdater = chunkManager.BorrowLightUpdater();

            if (blockIndex == Block.EmptyIndex)
            {
                lightUpdater.UpdateSkylightLevelByBlockRemoved(ref BlockPosition);
            }
            else
            {
                lightUpdater.UpdateSkylightLevelByBlockCreated(ref BlockPosition);
            }

            for (int i = 0; i < lightUpdater.AffectedChunkPositions.Count; i++)
            {
                var p = lightUpdater.AffectedChunkPositions[i];
                chunkManager.RequestUpdateMesh(ref p, ChunkMeshUpdatePriorities.High);
            }

            chunkManager.ReturnLightUpdater(lightUpdater);
        }
    }
}
