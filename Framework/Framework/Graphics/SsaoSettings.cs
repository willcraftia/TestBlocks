#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class SsaoSettings
    {
        public const float DefaultMapScale = 1;

        float mapScale = DefaultMapScale;

        /// <summary>
        /// ブラー設定を取得します。
        /// </summary>
        public BlurSettings Blur { get; private set; }

        /// <summary>
        /// 実スクリーンに対する法線深度マップのスケールを取得または設定します。
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

        public SsaoSettings()
        {
            Blur = new BlurSettings();
        }
    }
}
