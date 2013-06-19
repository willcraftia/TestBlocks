#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    /// <summary>
    /// メッシュ更新の優先度を示す列挙型です。
    /// </summary>
    public enum ChunkMeshUpdatePriority
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
        Low     = 2,
    }
}
