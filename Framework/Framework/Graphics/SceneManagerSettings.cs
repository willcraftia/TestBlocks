#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class SceneManagerSettings
    {
        /// <summary>
        /// 影設定。
        /// </summary>
        public ShadowSettings Shadow { get; private set; }

        /// <summary>
        /// 被写界深度設定。
        /// </summary>
        public DofSettings Dof { get; private set; }

        /// <summary>
        /// エッジ強調設定。
        /// </summary>
        public EdgeSettings Edge { get; private set; }

        /// <summary>
        /// スクリーン スペース アンビエント オクルージョン設定。
        /// </summary>
        public SsaoSettings Ssao { get; private set; }

        /// <summary>
        /// ブルーム設定。
        /// </summary>
        public BloomSettings Bloom { get; private set; }

        public SceneManagerSettings()
        {
            Shadow = new ShadowSettings();
            Dof = new DofSettings();
            Edge = new EdgeSettings();
            Ssao = new SsaoSettings();
            Bloom = new BloomSettings();
        }
    }
}
