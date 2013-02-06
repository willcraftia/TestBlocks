#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    /// <summary>
    /// チャンクの内部状態を構築するためのタスクの種類を定義します。
    /// </summary>
    public enum ChunkTaskTypes
    {
        /// <summary>
        /// チャンク ローカルで光レベルを構築。
        /// </summary>
        BuildLocalLights,

        /// <summary>
        /// 隣接チャンクからの光の拡散を考慮して光レベルを構築。
        /// </summary>
        /// <remarks>
        /// PropagateLights の要求は、
        /// BuildLocalLights を終えた後で無ければなりません。
        /// </remarks>
        PropagateLights
    }
}
