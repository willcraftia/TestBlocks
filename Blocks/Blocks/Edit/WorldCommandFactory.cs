#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class WorldCommandFactory
    {
        #region SetBlockCommand

        public sealed class SetBlockCommand : Command
        {
            public VectorI3 BlockPosition;

            public byte BlockIndex;

            ChunkManager chunkManager;

            ConcurrentPool<SetBlockCommand> pool;

            byte lastBlockIndex;

            public SetBlockCommand(ConcurrentPool<SetBlockCommand> pool, ChunkManager chunkManager)
            {
                this.pool = pool;
                this.chunkManager = chunkManager;
            }

            public override bool Do()
            {
                SetAndUpdateBlock(BlockIndex);
                return true;
            }

            public override void Undo()
            {
                SetAndUpdateBlock(lastBlockIndex);
            }

            public override void Release()
            {
                pool.Return(this);
            }

            void SetAndUpdateBlock(byte blockIndex)
            {
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

        #endregion

        WorldManager worldManager;

        ConcurrentPool<SetBlockCommand> setBlockCommandPool;

        public WorldCommandFactory(WorldManager worldManager)
        {
            if (worldManager == null) throw new ArgumentNullException("worldManager");

            this.worldManager = worldManager;

            setBlockCommandPool = new ConcurrentPool<SetBlockCommand>(CreatePooledSetBlockCommand);
        }

        public SetBlockCommand CreateSetBlockCommand()
        {
            return setBlockCommandPool.Borrow();
        }

        SetBlockCommand CreatePooledSetBlockCommand()
        {
            return new SetBlockCommand(setBlockCommandPool, worldManager.ChunkManager);
        }
    }
}
