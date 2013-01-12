#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    /// <summary>
    /// 時間と色の組を管理するクラスです。
    /// </summary>
    public sealed class TimeColor
    {
        /// <summary>
        /// 時間を取得または設定します。
        /// </summary>
        public float Time { get; set; }

        /// <summary>
        /// 色を取得または設定します。
        /// </summary>
        public Vector3 Color { get; set; }

        /// <summary>
        /// 指定の時間と色でインスタンスを生成します。
        /// </summary>
        /// <param name="time">時間。</param>
        /// <param name="color">色。</param>
        public TimeColor(float time, Vector3 color)
        {
            Time = time;
            Color = color;
        }

        #region ToString

        public override string ToString()
        {
            return "[Time: " + Time + ", Color: " + Color + "]";
        }

        #endregion
    }
}
