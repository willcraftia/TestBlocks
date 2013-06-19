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

            // メッシュ再構築。
            chunkManager.RequestUpdateMesh(chunk, ChunkMeshUpdatePriority.High);

            // 影響を受ける隣接チャンクがあるならば、それらもメッシュ再構築。
            var chunkBlock = new ChunkBlock(chunk, relativePosition);
            for (int i = 0; i < Side.Count; i++)
            {
                var neighborChunkBlock = chunkBlock.GetNeighbor(Side.Items[i]);
                if (neighborChunkBlock.Chunk != null && neighborChunkBlock.Chunk != chunkBlock.Chunk)
                {
                    chunkManager.RequestUpdateMesh(neighborChunkBlock.Chunk, ChunkMeshUpdatePriority.High);
                }
            }
        }
    }
}
