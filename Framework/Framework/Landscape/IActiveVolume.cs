#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    /// <summary>
    /// アクティブ パーティション領域へのインタフェースです。
    /// このクラスの実装では、視点位置を原点としたオフセット値としてパーティション位置を管理します。
    /// </summary>
    public interface IActiveVolume
    {
        /// <summary>
        /// パーティションの位置が領域に含まれるか否かを検査します。
        /// </summary>
        /// <param name="eyePosition">パーティション空間における視点位置。</param>
        /// <param name="point">パーティション空間におけるパーティションの位置。</param>
        /// <returns>
        /// true (パーティションの位置が領域に含まれる場合)、false (それ以外の場合)。
        /// </returns>
        bool Contains(ref VectorI3 eyePosition, ref VectorI3 point);

        /// <summary>
        /// 領域に含まれるパーティション位置に対して指定のメソッドを実行します。
        /// メソッドに渡されるパーティション位置はオフセット値です。
        /// </summary>
        /// <param name="action">実行するメソッド。</param>
        void ForEach(ForEachAction action);
    }
}
