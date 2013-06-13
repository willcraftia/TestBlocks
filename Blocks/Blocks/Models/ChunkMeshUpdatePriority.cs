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
        /// 通常優先度。
        /// </summary>
        Normal,

        /// <summary>
        /// 高優先度。
        /// </summary>
        High
    }
}
