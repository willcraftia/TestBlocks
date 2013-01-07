#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class ShadowSettings
    {
        public const bool DefaultSssmEnabled = false;

        /// <summary>
        /// シャドウ マップ設定を取得します。
        /// </summary>
        public ShadowMapSettings ShadowMap { get; private set; }

        /// <summary>
        /// スクリーン スペース シャドウ マッピングが有効か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (スクリーン スペース シャドウ マッピングが有効な場合)、false (それ以外の場合)。
        /// </value>
        public bool SssmEnabled { get; set; }

        /// <summary>
        /// スクリーン スペース シャドウ マッピング設定を取得します。
        /// </summary>
        public Sssm.Settings Sssm { get; private set; }

        public ShadowSettings()
        {
            ShadowMap = new ShadowMapSettings();

            SssmEnabled = DefaultSssmEnabled;
            Sssm = new Sssm.Settings();
        }
    }
}
