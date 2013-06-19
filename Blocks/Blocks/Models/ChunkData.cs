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
        ChunkManager manager;

        /// <summary>
        /// チャンクが参照するブロックのインデックス。
        /// </summary>
        byte[] blockIndices;

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
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="manager">チャンク マネージャ。</param>
        public ChunkData(ChunkManager manager)
        {
            if (manager == null) throw new ArgumentNullException("manager");

            this.manager = manager;

            blockIndices = new byte[manager.ChunkSize.X * manager.ChunkSize.Y * manager.ChunkSize.Z];
        }

        /// <summary>
        /// ブロックのインデックスを取得します。
        /// </summary>
        /// <param name="x">ブロックの X 位置。</param>
        /// <param name="y">ブロックの Y 位置。</param>
        /// <param name="z">ブロックの Z 位置。</param>
        /// <returns>ブロックのインデックス。</returns>
        public byte GetBlockIndex(int x, int y, int z)
        {
            if ((uint) manager.ChunkSize.X <= (uint) x ||
                (uint) manager.ChunkSize.Y <= (uint) y ||
                (uint) manager.ChunkSize.Z <= (uint) z)
                throw new ArgumentOutOfRangeException("position");

            var index = GetArrayIndex(x, y, z);
            return blockIndices[index];
        }

        /// <summary>
        /// ブロックのインデックスを設定します。
        /// </summary>
        /// <param name="x">ブロックの X 位置。</param>
        /// <param name="y">ブロックの Y 位置。</param>
        /// <param name="z">ブロックの Z 位置。</param>
        /// <param name="blockIndex">ブロックのインデックス。</param>
        public void SetBlockIndex(int x, int y, int z, byte blockIndex)
        {
            if ((uint) manager.ChunkSize.X <= (uint) x ||
                (uint) manager.ChunkSize.Y <= (uint) y ||
                (uint) manager.ChunkSize.Z <= (uint) z)
                throw new ArgumentOutOfRangeException("position");

            var index = GetArrayIndex(x, y, z);
            if (blockIndices[index] == blockIndex) return;

            blockIndices[index] = blockIndex;

            if (blockIndex != Block.EmptyIndex)
            {
                SolidCount++;
            }
            else
            {
                SolidCount--;
            }
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
            SolidCount = reader.ReadInt32();

            if (0 < SolidCount)
            {
                int solidCountValidation = 0;

                for (int i = 0; i < blockIndices.Length; i++)
                {
                    var value = reader.ReadByte();
                    blockIndices[i] = value;

                    if (value != Block.EmptyIndex)
                        solidCountValidation++;
                }

                if (solidCountValidation != SolidCount)
                    throw new InvalidDataException("Data corrupted.");
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(SolidCount);

            if (0 < SolidCount)
            {
                for (int i = 0; i < blockIndices.Length; i++)
                    writer.Write(blockIndices[i]);
            }
        }

        /// <summary>
        /// 指定のブロック位置を示すブロック配列のインデックスを取得します。
        /// </summary>
        /// <param name="x">ブロックの X 位置。</param>
        /// <param name="y">ブロックの Y 位置。</param>
        /// <param name="z">ブロックの Z 位置。</param>
        /// <returns>ブロック配列のインデックス。</returns>
        int GetArrayIndex(int x, int y, int z)
        {
            return x + y * manager.ChunkSize.X + z * manager.ChunkSize.X * manager.ChunkSize.Y;
        }
    }
}
