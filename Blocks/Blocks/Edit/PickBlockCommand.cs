﻿#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class PickBlockCommand : BrushCommand
    {
        byte lastSelectedBlockIndex;

        internal PickBlockCommand() { }

        public override bool Do()
        {
            var brush = BrushManager.ActiveBrush;
            if (brush == null) return false;

            var chunkManager = BrushManager.WorldManager.ChunkManager;

            var chunk = chunkManager.GetChunkByBlockPosition(brush.Position);
            if (chunk == null) return false;

            var relativeBlockPosition = chunk.GetRelativeBlockPosition(brush.Position);

            var blockIndex = chunk.GetBlockIndex(relativeBlockPosition);
            if (blockIndex == Block.EmptyIndex) return false;

            lastSelectedBlockIndex = BrushManager.SelectedBlockIndex;
            BrushManager.SelectedBlockIndex = blockIndex;

            return true;
        }

        public override void Undo()
        {
            BrushManager.SelectedBlockIndex = lastSelectedBlockIndex;
            lastSelectedBlockIndex = 0;

            base.Undo();
        }
    }
}
