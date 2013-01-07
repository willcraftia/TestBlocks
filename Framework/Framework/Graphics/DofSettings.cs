#region Using

using System;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class DofSettings
    {
        public const float DefaultMapScale = 0.5f;

        float mapScale = DefaultMapScale;

        /// <summary>
        /// ブラー設定を取得します。
        /// </summary>
        public BlurSettings Blur { get; private set; }

        /// <summary>
        /// 実スクリーンに対する深度マップのスケールを取得または設定します。
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

        public DofSettings()
        {
            Blur = new BlurSettings();
        }
    }
}
