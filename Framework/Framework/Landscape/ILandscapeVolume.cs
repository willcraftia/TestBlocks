#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    /// <summary>
    /// アクティブ パーティション領域へのインタフェースです。
    /// </summary>
    public interface ILandscapeVolume
    {
        /// <summary>
        /// 領域の中心位置を取得または設定します。
        /// </summary>
        VectorI3 Center { get; set; }

        /// <summary>
        /// 領域に含まれる全てのパーティションの位置を取得します。
        /// </summary>
        /// <returns></returns>
        VectorI3[] GetPoints();

        /// <summary>
        /// パーティションの位置が領域に含まれるか否かを検査します。
        /// </summary>
        /// <param name="point">パーティションの位置。</param>
        /// <returns>
        /// true (パーティションの位置が領域に含まれる場合)、false (それ以外の場合)。
        /// </returns>
        bool Contains(VectorI3 point);

        void ForEach(Action<VectorI3> action);

        void ForEach(Func<VectorI3, bool> function);
    }
}
