#region Using

using System;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    /// <summary>
    /// 隣接チャンク集合を管理するクラスです。
    /// </summary>
    public sealed class CloseChunks
    {
        /// <summary>
        /// 隣接チャンク集合。
        /// (1, 1, 1) が中心位置。
        /// </summary>
        Chunk[, ,] chunks = new Chunk[3, 3, 3];

        /// <summary>
        /// 指定の位置のチャンクを取得または設定します。
        /// インデックスは、(0, 0, 0) を中心位置としたオフセット値として [-1, 1] で指定します。
        /// </summary>
        /// <param name="x">X オフセット。</param>
        /// <param name="y">Y オフセット</param>
        /// <param name="z">Z オフセット。</param>
        /// <returns>チャンク。</returns>
        public Chunk this[int x, int y, int z]
        {
            get { return chunks[x + 1, y + 1, z + 1]; }
            set { chunks[x + 1, y + 1, z + 1] = value; }
        }

        /// <summary>
        /// 状態を初期化します。
        /// </summary>
        public void Clear()
        {
            Array.Clear(chunks, 0, chunks.Length);
        }
    }
}
