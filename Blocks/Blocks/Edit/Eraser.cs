#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class Eraser
    {
        CommandManager commandManager;

        WorldCommandFactory commandFactory;

        ChunkManager chunkManager;

        public VectorI3 Position;

        public Eraser(CommandManager commandManager, WorldCommandFactory commandFactory,
            ChunkManager chunkManager)
        {
            if (commandManager == null) throw new ArgumentNullException("commandManager");
            if (commandFactory == null) throw new ArgumentNullException("commandFactory");
            if (chunkManager == null) throw new ArgumentNullException("chunkManager");

            this.commandManager = commandManager;
            this.commandFactory = commandFactory;
            this.chunkManager = chunkManager;
        }

        public void Erase()
        {
            var chunkPosition = chunkManager.GetChunkPositionByBlockPosition(Position);
            var chunk = chunkManager[chunkPosition] as Chunk;
            var relativePosition = chunk.GetRelativeBlockPosition(Position);

            // 既にブロックが存在しないならば抑制。
            if (chunk.GetBlockIndex(ref relativePosition) == Block.EmptyIndex) return;

            var command = commandFactory.CreateSetBlockCommand();

            command.BlockPosition = Position;
            command.BlockIndex = Block.EmptyIndex;

            commandManager.RequestCommand(command);
        }
    }
}
