#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class ShadowSettings
    {
        public const bool DefaultShadowEnabled = false;

        bool shadowEnabled;

        /// <summary>
        /// 影が有効かどうか。
        /// </summary>
        /// <value>
        /// true (影が有効)、false (それ以外の場合)。
        /// </value>
        public bool ShadowEnabled
        {
            get { return shadowEnabled; }
            set { shadowEnabled = value; }
        }

        /// <summary>
        /// シャドウ マップの設定。
        /// </summary>
        public ShadowMapSettings ShadowMap { get; private set; }

        /// <summary>
        /// ライト カメラの設定。
        /// </summary>
        public LightFrustumSettings LightFrustum { get; private set; }

        public ShadowSettings()
        {
            ShadowMap = new ShadowMapSettings();
            LightFrustum = new LightFrustumSettings();
        }
    }
}
