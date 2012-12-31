﻿#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class ShadowSettings
    {
        public const bool DefaultEnabled = true;

        bool enabled = DefaultEnabled;

        /// <summary>
        /// シャドウ処理が有効かどうかを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (シャドウ処理が有効)、false (それ以外の場合)。
        /// </value>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// シャドウ マップ設定を取得します。
        /// </summary>
        public ShadowMapSettings ShadowMap { get; private set; }

        /// <summary>
        /// ライト カメラ設定を取得します。
        /// </summary>
        public LightFrustumSettings LightFrustum { get; private set; }

        /// <summary>
        /// シャドウ シーン設定を取得します。
        /// </summary>
        public ShadowSceneSettings ShadowScene { get; private set; }

        public ShadowSettings()
        {
            ShadowMap = new ShadowMapSettings();
            LightFrustum = new LightFrustumSettings();
            ShadowScene = new ShadowSceneSettings();
        }
    }
}
