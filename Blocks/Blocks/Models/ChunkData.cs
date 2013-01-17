#region Using

using System;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkData
    {
        /// <summary>
        /// チャンク マネージャ。
        /// </summary>
        ChunkManager chunkManager;

        /// <summary>
        /// チャンクのサイズ。
        /// </summary>
        VectorI3 size;

        /// <summary>
        /// チャンクが参照するブロックのインデックス。
        /// </summary>
        byte[] blockIndices;

        /// <summary>
        /// ブロックのインデックスを取得または設定します。
        /// ブロック位置は、チャンク空間における相対座標で指定します。
        /// </summary>
        /// <param name="x">チャンク空間における相対ブロック位置 X。</param>
        /// <param name="y">チャンク空間における相対ブロック位置 Y。</param>
        /// <param name="z">チャンク空間における相対ブロック位置 Z。</param>
        /// <returns></returns>
        public byte this[int x, int y, int z]
        {
            get
            {
                if (x < 0 || size.X < x) throw new ArgumentOutOfRangeException("x");
                if (y < 0 || size.Y < y) throw new ArgumentOutOfRangeException("y");
                if (z < 0 || size.Z < z) throw new ArgumentOutOfRangeException("z");

                var index = x + y * size.X + z * size.X * size.Y;
                return blockIndices[index];
            }
            set
            {
                if (x < 0 || size.X < x) throw new ArgumentOutOfRangeException("x");
                if (y < 0 || size.Y < y) throw new ArgumentOutOfRangeException("y");
                if (z < 0 || size.Z < z) throw new ArgumentOutOfRangeException("z");

                var index = x + y * size.X + z * size.X * size.Y;

                if (blockIndices[index] == value) return;

                blockIndices[index] = value;
                Dirty = true;
            }
        }

        /// <summary>
        /// ブロックの総数を取得します。
        /// </summary>
        public int Count
        {
            get { return blockIndices.Length; }
        }

        public bool Dirty { get; set; }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="chunkManager">チャンク マネージャ。</param>
        public ChunkData(ChunkManager chunkManager)
        {
            if (chunkManager == null) throw new ArgumentNullException("chunkManager");

            this.chunkManager = chunkManager;

            size = chunkManager.ChunkSize;

            blockIndices = new byte[size.X * size.Y * size.Z];
        }

        public void Clear()
        {
            Array.Clear(blockIndices, 0, blockIndices.Length);
        }
    }
}
