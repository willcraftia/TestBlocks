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
                var chunk = chunkManager.GetChunkByBlockPosition(ref BlockPosition);
                if (chunk == null) throw new InvalidOperationException("Chunk not found: BlockPosition=" + BlockPosition);

                VectorI3 relativePosition;
                chunk.GetRelativeBlockPosition(ref BlockPosition, out relativePosition);

                lastBlockIndex = chunk.GetBlockIndex(ref relativePosition);

                chunk.SetBlockIndex(ref relativePosition, BlockIndex);

                var lightUpdater = chunkManager.BorrowLightUpdater();

                if (BlockIndex == Block.EmptyIndex)
                {
                    lightUpdater.UpdateSkylightLevelByBlockRemoved(ref BlockPosition);
                }
                else
                {
                    lightUpdater.UpdateSkylightLevelByBlockCreated(ref BlockPosition);
                }

                foreach (var chunkPosition in lightUpdater.AffectedChunkPositions)
                {
                    var p = chunkPosition;
                    chunkManager.RequestUpdateMesh(ref p, ChunkMeshUpdatePriorities.High);
                }

                chunkManager.ReturnLightUpdater(lightUpdater);

                return true;
            }

            public override void Undo()
            {
                var chunk = chunkManager.GetChunkByBlockPosition(ref BlockPosition);
                if (chunk == null) throw new InvalidOperationException("Chunk not found: BlockPosition=" + BlockPosition);

                VectorI3 relativePosition;
                chunk.GetRelativeBlockPosition(ref BlockPosition, out relativePosition);

                chunk.SetBlockIndex(ref relativePosition, lastBlockIndex);

                var bounds = BoundingBoxI.CreateFromCenterExtents(chunk.Position, VectorI3.One);
                chunkManager.RequestChunkTask(ref bounds, ChunkTaskTypes.BuildLocalLights, ChunkTaskPriorities.High);
            }

            public override void Release()
            {
                pool.Return(this);
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
