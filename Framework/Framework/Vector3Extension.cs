#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework
{
    /// <summary>
    /// Vector3 の拡張クラスです。
    /// </summary>
    public static class Vector3Extension
    {
        /// <summary>
        /// ゼロ ベクトルであるか否かを検査します。
        /// </summary>
        /// <param name="vector">Vector3。</param>
        /// <returns>
        /// true (ゼロ ベクトルである場合)、false (それ以外の場合)。
        /// </returns>
        public static bool IsZero(this Vector3 vector)
        {
            return vector.X == 0 && vector.Y == 0 && vector.Z == 0;
        }
    }
}
