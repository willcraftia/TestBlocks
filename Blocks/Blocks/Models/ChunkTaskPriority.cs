#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    /// <summary>
    /// チャンク タスクの優先度を定義する列挙型です。
    /// </summary>
    public enum ChunkTaskPriority
    {
        /// <summary>
        /// 高優先度。
        /// </summary>
        High    = 0,

        /// <summary>
        /// 通常優先度。
        /// </summary>
        Normal  = 1,

        /// <summary>
        /// 低優先度。
        /// </summary>
        Low     = 2
    }
}
