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

            public int UpdateMeshPriority = ChunkManager.SystemEditUpdateMeshPriority;

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
                var chunk = chunkManager.GetChunkByBlockPosition(BlockPosition);
                if (chunk == null) throw new InvalidOperationException("Chunk not found: BlockPosition=" + BlockPosition);

                var relativePosition = chunk.GetRelativeBlockPosition(BlockPosition);

                lastBlockIndex = chunk[relativePosition];

                chunk[relativePosition] = BlockIndex;
                chunkManager.RequestUpdateMesh(chunk.Position, UpdateMeshPriority);

                return true;
            }

            public override void Undo()
            {
                var chunk = chunkManager.GetChunkByBlockPosition(BlockPosition);
                if (chunk == null) throw new InvalidOperationException("Chunk not found: BlockPosition=" + BlockPosition);

                var relativePosition = chunk.GetRelativeBlockPosition(BlockPosition);

                chunk[relativePosition] = lastBlockIndex;
                chunkManager.RequestUpdateMesh(chunk.Position, UpdateMeshPriority);
            }

            public override void Release()
            {
                UpdateMeshPriority = ChunkManager.SystemEditUpdateMeshPriority;

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
