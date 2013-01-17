#region Using

using System;
using System.IO;
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
                if (x < 0 || chunkManager.ChunkSize.X < x) throw new ArgumentOutOfRangeException("x");
                if (y < 0 || chunkManager.ChunkSize.Y < y) throw new ArgumentOutOfRangeException("y");
                if (z < 0 || chunkManager.ChunkSize.Z < z) throw new ArgumentOutOfRangeException("z");

                var index = x + y * chunkManager.ChunkSize.X + z * chunkManager.ChunkSize.X * chunkManager.ChunkSize.Y;
                return blockIndices[index];
            }
            set
            {
                if (x < 0 || chunkManager.ChunkSize.X < x) throw new ArgumentOutOfRangeException("x");
                if (y < 0 || chunkManager.ChunkSize.Y < y) throw new ArgumentOutOfRangeException("y");
                if (z < 0 || chunkManager.ChunkSize.Z < z) throw new ArgumentOutOfRangeException("z");

                var index = x + y * chunkManager.ChunkSize.X + z * chunkManager.ChunkSize.X * chunkManager.ChunkSize.Y;

                if (blockIndices[index] == value) return;

                blockIndices[index] = value;
                Dirty = true;

                if (value != Block.EmptyIndex)
                {
                    SolidCount++;
                }
                else
                {
                    SolidCount--;
                }
            }
        }

        /// <summary>
        /// ブロックの総数を取得します。
        /// </summary>
        public int Count
        {
            get { return blockIndices.Length; }
        }

        /// <summary>
        /// 非空ブロックの総数を取得します。
        /// </summary>
        public int SolidCount { get; private set; }

        /// <summary>
        /// ブロックのインデックス配列に変更があったか否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (ブロックのインデックス配列に変更があった場合)、false (それ以外の場合)。
        /// </value>
        public bool Dirty { get; private set; }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="chunkManager">チャンク マネージャ。</param>
        public ChunkData(ChunkManager chunkManager)
        {
            if (chunkManager == null) throw new ArgumentNullException("chunkManager");

            this.chunkManager = chunkManager;

            blockIndices = new byte[chunkManager.ChunkSize.X * chunkManager.ChunkSize.Y * chunkManager.ChunkSize.Z];
        }

        /// <summary>
        /// 初期化します。
        /// </summary>
        public void Clear()
        {
            Array.Clear(blockIndices, 0, blockIndices.Length);
            SolidCount = 0;
        }

        public void Read(BinaryReader reader)
        {
            //var p = new VectorI3();

            //p.X = reader.ReadInt32();
            //p.Y = reader.ReadInt32();
            //p.Z = reader.ReadInt32();

            for (int i = 0; i < blockIndices.Length; i++)
                blockIndices[i] = reader.ReadByte();
        }

        public void Write(BinaryWriter writer)
        {
            //writer.Write(position.X);
            //writer.Write(position.Y);
            //writer.Write(position.Z);

            for (int i = 0; i < blockIndices.Length; i++)
                writer.Write(blockIndices[i]);
        }
    }
}
