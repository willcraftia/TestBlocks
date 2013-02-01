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
                chunkManager.RequestUpdateMesh(ref chunk.Position, ChunkManager.UpdateMeshPriority.High);

                RequestUpdateMeshForNeighbors(ref chunk.Position, ref relativePosition);

                return true;
            }

            public override void Undo()
            {
                var chunk = chunkManager.GetChunkByBlockPosition(ref BlockPosition);
                if (chunk == null) throw new InvalidOperationException("Chunk not found: BlockPosition=" + BlockPosition);

                VectorI3 relativePosition;
                chunk.GetRelativeBlockPosition(ref BlockPosition, out relativePosition);

                chunk.SetBlockIndex(ref relativePosition, lastBlockIndex);
                chunkManager.RequestUpdateMesh(ref chunk.Position, ChunkManager.UpdateMeshPriority.High);

                RequestUpdateMeshForNeighbors(ref chunk.Position, ref relativePosition);
            }

            public override void Release()
            {
                pool.Return(this);
            }

            void RequestUpdateMeshForNeighbors(ref VectorI3 baseChunkPosition, ref VectorI3 blockPosition)
            {
                if (blockPosition.X == 0)
                {
                    RequestUpdateMeshForNeighbor(ref baseChunkPosition, CubicSide.Left);
                }
                else if (blockPosition.X == (chunkManager.ChunkSize.X - 1))
                {
                    RequestUpdateMeshForNeighbor(ref baseChunkPosition, CubicSide.Right);
                }

                if (blockPosition.Y == 0)
                {
                    RequestUpdateMeshForNeighbor(ref baseChunkPosition, CubicSide.Bottom);
                }
                else if (blockPosition.Y == (chunkManager.ChunkSize.Y - 1))
                {
                    RequestUpdateMeshForNeighbor(ref baseChunkPosition, CubicSide.Top);
                }

                if (blockPosition.Z == 0)
                {
                    RequestUpdateMeshForNeighbor(ref baseChunkPosition, CubicSide.Back);
                }
                else if (blockPosition.Z == (chunkManager.ChunkSize.Z - 1))
                {
                    RequestUpdateMeshForNeighbor(ref baseChunkPosition, CubicSide.Front);
                }
            }

            void RequestUpdateMeshForNeighbor(ref VectorI3 basePosition, CubicSide side)
            {
                var neighborPosition = basePosition + side.Direction;
                if (chunkManager.Contains(ref neighborPosition))
                    chunkManager.RequestUpdateMesh(ref neighborPosition, ChunkManager.UpdateMeshPriority.High);
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
