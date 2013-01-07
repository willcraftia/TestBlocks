#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class SceneManagerSettings
    {
        public const bool DefaultShadowEnabled = true;

        public const bool DefaultSsaoEnabled = true;

        public const bool DefaultEdgeEnabled = false;

        public const bool DefaultBloomEnabled = false;

        public const bool DefaultDofEnabled = true;

        public const bool DefaultColorOverlapEnabled = false;

        public const bool DefaultMonochromeEnabled = false;

        /// <summary>
        /// シャドウ処理が有効かどうかを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (シャドウ処理が有効)、false (それ以外の場合)。
        /// </value>
        public bool ShadowEnabled { get; set; }

        /// <summary>
        /// シャドウ設定を取得します。
        /// </summary>
        public ShadowSettings Shadow { get; private set; }

        /// <summary>
        /// スクリーン スペース アンビエント オクルージョンが有効か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (スクリーン スペース アンビエント オクルージョンが有効な場合)、false (それ以外の場合)。
        /// </value>
        public bool SsaoEnabled { get; set; }

        /// <summary>
        /// スクリーン スペース アンビエント オクルージョン設定を取得します。
        /// </summary>
        public Ssao.Settings Ssao { get; private set; }

        /// <summary>
        /// エッジ強調が有効か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (エッジ強調が有効な場合)、false (それ以外の場合)。
        /// </value>
        public bool EdgeEnabled { get; set; }

        /// <summary>
        /// エッジ強調設定を取得します。
        /// </summary>
        public Edge.Settings Edge { get; private set; }

        /// <summary>
        /// ブルームが有効か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (ブルームが有効な場合)、false (それ以外の場合)。
        /// </value>
        public bool BloomEnabled { get; set; }

        /// <summary>
        /// ブルーム設定を取得します。
        /// </summary>
        public Bloom.Settings Bloom { get; private set; }

        /// <summary>
        /// 被写界深度が有効か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (被写界深度が有効な場合)、false (それ以外の場合)。
        /// </value>
        public bool DofEnabled { get; set; }

        /// <summary>
        /// 被写界深度設定を取得します。
        /// </summary>
        public Dof.Settings Dof { get; private set; }

        /// <summary>
        /// カラー オーバラップが有効か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (カラー オーバラップが有効な場合)、false (それ以外の場合)。
        /// </value>
        public bool ColorOverlapEnabled { get; set; }

        /// <summary>
        /// モノクロームが有効か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (モノクロームが有効な場合)、false (それ以外の場合)。
        /// </value>
        public bool MonochromeEnabled { get; set; }

        public SceneManagerSettings()
        {
            ShadowEnabled = DefaultShadowEnabled;
            Shadow = new ShadowSettings();

            SsaoEnabled = DefaultSsaoEnabled;
            Ssao = new Ssao.Settings();

            EdgeEnabled = DefaultEdgeEnabled;
            Edge = new Edge.Settings();

            BloomEnabled = DefaultBloomEnabled;
            Bloom = new Bloom.Settings();

            DofEnabled = DefaultDofEnabled;
            Dof = new Dof.Settings();

            ColorOverlapEnabled = DefaultColorOverlapEnabled;
            
            MonochromeEnabled = DefaultMonochromeEnabled;
        }
    }
}
