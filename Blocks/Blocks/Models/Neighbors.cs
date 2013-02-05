#region Using

using System;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    /// <summary>
    /// グリッドに沿った 6 方向で隣接する要素を管理するための構造体です。
    /// </summary>
    /// <remarks>
    /// この構造体は、隣接要素を方向と共に管理するための構造を持つのみであり、
    /// 各方向へ適切な要素を設定することは利用側の責務です。
    /// </remarks>
    /// <typeparam name="T">要素の型。</typeparam>
    public struct Neighbors<T>
    {
        /// <summary>
        /// 上方向 (Y 軸正の方向) で隣接する要素。
        /// </summary>
        public T Top;

        /// <summary>
        /// 下方向 (Y 軸負の方向) で隣接する要素。
        /// </summary>
        public T Bottom;

        /// <summary>
        /// 前方向 (Z 軸正の方向) で隣接する要素。
        /// </summary>
        public T Front;

        /// <summary>
        /// 後方向 (Z 軸負の方向) で隣接する要素。
        /// </summary>
        public T Back;

        /// <summary>
        /// 左方向 (X 軸負の方向) で隣接する要素。
        /// </summary>
        public T Left;

        /// <summary>
        /// 右方向 (X 軸正の方向) で隣接する要素。
        /// </summary>
        public T Right;

        /// <summary>
        /// 指定の方向の要素を取得または設定します。
        /// </summary>
        /// <param name="side">方向。</param>
        /// <returns>要素。</returns>
        public T this[CubicSide side]
        {
            get
            {
                if (side == CubicSide.Top) return Top;
                if (side == CubicSide.Bottom) return Bottom;
                if (side == CubicSide.Front) return Front;
                if (side == CubicSide.Back) return Back;
                if (side == CubicSide.Left) return Left;
                if (side == CubicSide.Right) return Right;
                throw new InvalidOperationException();
            }
            set
            {
                if (side == CubicSide.Top) { Top = value; return; }
                if (side == CubicSide.Bottom) { Bottom = value; return; }
                if (side == CubicSide.Front) { Front = value; return; }
                if (side == CubicSide.Back) { Back = value; return; }
                if (side == CubicSide.Left) { Left = value; return; }
                if (side == CubicSide.Right) { Right = value; return; }
            }
        }
    }
}
