#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class SceneManagerSettings
    {
        public const bool DefaultColorOverlapEnabled = true;

        /// <summary>
        /// 影設定。
        /// </summary>
        public ShadowSettings Shadow { get; private set; }

        /// <summary>
        /// スクリーン スペース アンビエント オクルージョン設定。
        /// </summary>
        public SsaoSettings Ssao { get; private set; }

        /// <summary>
        /// エッジ強調設定。
        /// </summary>
        public EdgeSettings Edge { get; private set; }

        /// <summary>
        /// ブルーム設定。
        /// </summary>
        public BloomSettings Bloom { get; private set; }

        /// <summary>
        /// 被写界深度設定。
        /// </summary>
        public DofSettings Dof { get; private set; }

        /// <summary>
        /// カラー オーバラップが有効か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (カラー オーバラップが有効な場合)、false (それ以外の場合)。
        /// </value>
        public bool ColorOverlapEnabled { get; set; }

        public SceneManagerSettings()
        {
            Shadow = new ShadowSettings();
            Ssao = new SsaoSettings();
            Edge = new EdgeSettings();
            Bloom = new BloomSettings();
            Dof = new DofSettings();

            ColorOverlapEnabled = DefaultColorOverlapEnabled;
        }
    }
}
