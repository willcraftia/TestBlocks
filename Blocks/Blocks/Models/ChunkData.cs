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
        /// 天空光レベルの配列。
        /// </summary>
        HalfByteArray3 skylightLevels;

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
            skylightLevels = new HalfByteArray3(manager.ChunkSize.X, manager.ChunkSize.Y, manager.ChunkSize.Z);
            
            // 初期状態は空チャンクであるため、最大光レベルで充満させる。
            ClearSkylightLevels();
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
        /// 天空光レベルを取得します。
        /// </summary>
        /// <param name="x">ブロックの X 位置。</param>
        /// <param name="y">ブロックの Y 位置。</param>
        /// <param name="z">ブロックの Z 位置。</param>
        /// <returns>天空光レベル。</returns>
        public byte GetSkylightLevel(int x, int y, int z)
        {
            return skylightLevels[x, y, z];
        }

        /// <summary>
        /// 天空光レベルを設定します。
        /// </summary>
        /// <param name="x">ブロックの X 位置。</param>
        /// <param name="y">ブロックの Y 位置。</param>
        /// <param name="z">ブロックの Z 位置。</param>
        /// <param name="value">天空光レベル。</param>
        public void SetSkylightLevel(int x, int y, int z, byte value)
        {
            skylightLevels[x, y, z] = value;
        }

        /// <summary>
        /// 全ての位置に指定の天空光レベルを設定します。
        /// </summary>
        /// <param name="value">天空光レベル。</param>
        public void FillSkylightLevels(byte level)
        {
            skylightLevels.Fill(level);
        }

        /// <summary>
        /// 全ての位置の天空光レベルを 0 に設定します。
        /// </summary>
        public void ClearSkylightLevels()
        {
            FillSkylightLevels(Chunk.MaxSkylightLevel);
        }

        /// <summary>
        /// 初期化します。
        /// </summary>
        public void Clear()
        {
            Array.Clear(blockIndices, 0, blockIndices.Length);
            SolidCount = 0;

            ClearSkylightLevels();
        }

        public void Read(BinaryReader reader)
        {
            //var p = new VectorI3();

            //p.X = reader.ReadInt32();
            //p.Y = reader.ReadInt32();
            //p.Z = reader.ReadInt32();

            for (int i = 0; i < blockIndices.Length; i++)
            {
                var value = reader.ReadByte();
                blockIndices[i] = value;

                if (value != Block.EmptyIndex) SolidCount++;
            }
        }

        public void Write(BinaryWriter writer)
        {
            //writer.Write(position.X);
            //writer.Write(position.Y);
            //writer.Write(position.Z);

            for (int i = 0; i < blockIndices.Length; i++)
                writer.Write(blockIndices[i]);
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
