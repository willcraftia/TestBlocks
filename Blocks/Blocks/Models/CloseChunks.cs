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
        /// チャンクのサイズ。
        /// </summary>
        VectorI3 chunkSize;

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
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="chunkSize">チャンクのサイズ。</param>
        public CloseChunks(VectorI3 chunkSize)
        {
            if (chunkSize.X < 1 || chunkSize.Y < 1 || chunkSize.Z < 1)
                throw new ArgumentOutOfRangeException("chunkSize");

            this.chunkSize = chunkSize;
        }

        /// <summary>
        /// 状態を初期化します。
        /// </summary>
        public void Clear()
        {
            Array.Clear(chunks, 0, chunks.Length);
        }

        /// <summary>
        /// 指定の位置にあるブロックのインデックスを取得します。
        /// 指定するブロックの位置は、中心チャンク内における相対位置です。
        /// 指定の位置が中心チャンク内に無い場合、隣接チャンクから取得を試みますが、
        /// いずれの隣接チャンクにも含まれない場合には、未定として null を返します。
        /// </summary>
        /// <param name="blockPosition">中心チャンク内における相対ブロック位置。</param>
        /// <returns>
        /// ブロックのインデックス、あるいは、指定のブロック位置が範囲外ならば null。
        /// </returns>
        public byte? GetBlockIndex(ref VectorI3 blockPosition)
        {
            var x = (blockPosition.X < 0) ? -1 : (blockPosition.X < chunkSize.X) ? 0 : 1;
            var y = (blockPosition.Y < 0) ? -1 : (blockPosition.Y < chunkSize.Y) ? 0 : 1;
            var z = (blockPosition.Z < 0) ? -1 : (blockPosition.Z < chunkSize.Z) ? 0 : 1;

            // メモ
            //
            // 実際には、隣接チャンク集合には、
            // ゲーム スレッドにて非アクティブ化されてしまったものも含まれる可能性がある。
            // しかし、それら全てを同期することは負荷が高いため、
            // ここでは無視している。
            // なお、非アクティブ化されたものが含まれる場合、
            // 再度、メッシュ更新要求が発生するはずである。

            // ブロックが隣接チャンクに含まれている場合。
            var closeChunk = this[x, y, z];

            // 隣接チャンクがないならば未定として null。
            if (closeChunk == null) return null;

            // 隣接チャンクにおける相対ブロック座標を算出。
            var relativeX = blockPosition.X % chunkSize.X;
            var relativeY = blockPosition.Y % chunkSize.Y;
            var relativeZ = blockPosition.Z % chunkSize.Z;
            if (relativeX < 0) relativeX += chunkSize.X;
            if (relativeY < 0) relativeY += chunkSize.Y;
            if (relativeZ < 0) relativeZ += chunkSize.Z;

            return closeChunk[relativeX, relativeY, relativeZ];
        }
    }
}
