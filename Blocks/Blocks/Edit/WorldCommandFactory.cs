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

            public override void Do()
            {
                var chunk = chunkManager.GetChunkByBlockPosition(BlockPosition);
                if (chunk == null) throw new InvalidOperationException("Chunk not found: BlockPosition=" + BlockPosition);

                var relativePosition = chunk.GetRelativeBlockPosition(BlockPosition);

                lastBlockIndex = chunk[relativePosition];

                chunk[relativePosition] = BlockIndex;
            }

            public override void Undo()
            {
                var chunk = chunkManager.GetChunkByBlockPosition(BlockPosition);
                if (chunk == null) throw new InvalidOperationException("Chunk not found: BlockPosition=" + BlockPosition);

                var relativePosition = chunk.GetRelativeBlockPosition(BlockPosition);

                chunk[relativePosition] = lastBlockIndex;
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
