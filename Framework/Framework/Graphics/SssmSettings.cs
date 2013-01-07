#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class SssmSettings
    {
        public const float DefaultMapScale = 0.25f;

        float mapScale = DefaultMapScale;

        /// <summary>
        /// ブラーを適用するか否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (ブラーを適用する場合)、false (それ以外の場合)。
        /// </value>
        public bool BlurEnabled { get; set; }

        /// <summary>
        /// ブラー設定を取得します。
        /// </summary>
        public BlurSettings Blur { get; private set; }

        /// <summary>
        /// 実スクリーンに対するシャドウ シーンのスケールを取得または設定します。
        /// </summary>
        public float MapScale
        {
            get { return mapScale; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value");

                mapScale = value;
            }
        }

        public SssmSettings()
        {
            Blur = new BlurSettings();
        }
    }
}
