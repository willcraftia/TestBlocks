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
        /// ブロックのインデックス配列に変更があったか否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (ブロックのインデックス配列に変更があった場合)、false (それ以外の場合)。
        /// </value>
        public bool Dirty { get; private set; }

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
        }

        public byte GetBlockIndex(int x, int y, int z)
        {
            if (x < 0 || manager.ChunkSize.X < x ||
                y < 0 || manager.ChunkSize.Y < y ||
                z < 0 || manager.ChunkSize.Z < z)
                throw new ArgumentOutOfRangeException("position");

            var index = GetArrayIndex(x, y, z);
            return blockIndices[index];
        }

        /// <summary>
        /// ブロックのインデックスを取得します。
        /// </summary>
        /// <param name="position">ブロックの位置。</param>
        /// <returns>ブロックのインデックス。</returns>
        public byte GetBlockIndex(ref VectorI3 position)
        {
            if (position.X < 0 || manager.ChunkSize.X < position.X ||
                position.Y < 0 || manager.ChunkSize.Y < position.Y ||
                position.Z < 0 || manager.ChunkSize.Z < position.Z)
                throw new ArgumentOutOfRangeException("position");

            var index = GetArrayIndex(ref position);
            return blockIndices[index];
        }

        /// <summary>
        /// ブロックのインデックスを設定します。
        /// </summary>
        /// <param name="position">ブロックの位置。</param>
        /// <param name="blockIndex">ブロックのインデックス。</param>
        public void SetBlockIndex(ref VectorI3 position, byte blockIndex)
        {
            if (position.X < 0 || manager.ChunkSize.X < position.X ||
                position.Y < 0 || manager.ChunkSize.Y < position.Y ||
                position.Z < 0 || manager.ChunkSize.Z < position.Z)
                throw new ArgumentOutOfRangeException("position");

            var index = GetArrayIndex(ref position);
            if (blockIndices[index] == blockIndex) return;

            blockIndices[index] = blockIndex;
            Dirty = true;

            if (blockIndex != Block.EmptyIndex)
            {
                SolidCount++;
            }
            else
            {
                SolidCount--;
            }
        }

        public byte GetSkylightLevel(int x, int y, int z)
        {
            return skylightLevels[x, y, z];
        }

        public byte GetSkylightLevel(ref VectorI3 position)
        {
            return skylightLevels[position.X, position.Y, position.Z];
        }

        public void SetSkylightLevel(int x, int y, int z, byte value)
        {
            skylightLevels[x, y, z] = value;
        }

        public void SetSkylightLevel(ref VectorI3 position, byte value)
        {
            skylightLevels[position.X, position.Y, position.Z] = value;
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

        public void ClearSkylightLevels()
        {
            skylightLevels.Clear();
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

        int GetArrayIndex(int x, int y, int z)
        {
            return x + y * manager.ChunkSize.X + z * manager.ChunkSize.X * manager.ChunkSize.Y;
        }

        /// <summary>
        /// 指定のブロック位置を示すブロック配列のインデックスを取得します。
        /// </summary>
        /// <param name="position">ブロック位置。</param>
        /// <returns>ブロック配列のインデックス。</returns>
        int GetArrayIndex(ref VectorI3 position)
        {
            return position.X + position.Y * manager.ChunkSize.X + position.Z * manager.ChunkSize.X * manager.ChunkSize.Y;
        }
    }
}
