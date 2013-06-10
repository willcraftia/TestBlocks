﻿#region Using

using System;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    /// <summary>
    /// 隣接チャンクを管理するための構造体です。
    /// </summary>
    /// <remarks>
    /// この構造体は、方向と共に隣接チャンクを管理するための構造を持つのみであり、
    /// 各方向へ適切な隣接チャンクを設定することは利用側の責務です。
    /// </remarks>
    public struct ChunkNeighbors
    {
        /// <summary>
        /// 上方向 (Y 軸正の方向) で隣接するチャンク。
        /// </summary>
        public Chunk Top;

        /// <summary>
        /// 下方向 (Y 軸負の方向) で隣接するチャンク。
        /// </summary>
        public Chunk Bottom;

        /// <summary>
        /// 前方向 (Z 軸正の方向) で隣接するチャンク。
        /// </summary>
        public Chunk Front;

        /// <summary>
        /// 後方向 (Z 軸負の方向) で隣接するチャンク。
        /// </summary>
        public Chunk Back;

        /// <summary>
        /// 左方向 (X 軸負の方向) で隣接するチャンク。
        /// </summary>
        public Chunk Left;

        /// <summary>
        /// 右方向 (X 軸正の方向) で隣接するチャンク。
        /// </summary>
        public Chunk Right;

        /// <summary>
        /// 指定の方向のチャンクを取得または設定します。
        /// </summary>
        /// <param name="side">方向。</param>
        /// <returns>チャンク。</returns>
        public Chunk this[Side side]
        {
            get
            {
                if (side == Side.Top) return Top;
                if (side == Side.Bottom) return Bottom;
                if (side == Side.Front) return Front;
                if (side == Side.Back) return Back;
                if (side == Side.Left) return Left;
                if (side == Side.Right) return Right;
                throw new InvalidOperationException();
            }
            set
            {
                if (side == Side.Top) { Top = value; return; }
                if (side == Side.Bottom) { Bottom = value; return; }
                if (side == Side.Front) { Front = value; return; }
                if (side == Side.Back) { Back = value; return; }
                if (side == Side.Left) { Left = value; return; }
                if (side == Side.Right) { Right = value; return; }
            }
        }
    }
}
