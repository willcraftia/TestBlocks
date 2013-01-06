#region Using

using System;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class DofSettings
    {
        public const bool DefaultEnabled = true;

        public const float DefaultMapScale = 0.5f;

        public const SurfaceFormat DefaultFormat = SurfaceFormat.Single;

        bool enabled = DefaultEnabled;

        float mapScale = DefaultMapScale;

        SurfaceFormat format = DefaultFormat;

        float farPlaneDistance = PerspectiveFov.DefaultFarPlaneDistance;

        /// <summary>
        /// 被写界深度が有効か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (被写界深度が有効な場合)、false (それ以外の場合)。
        /// </value>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

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

        /// <summary>
        /// 深度マップの SurfaceFormat。
        /// </summary>
        public SurfaceFormat Format
        {
            get { return format; }
        }

        /// <summary>
        /// 深度マップ描画で使用するカメラの、遠くのビュー プレーンとの距離。
        /// </summary>
        public float FarPlaneDistance
        {
            get { return farPlaneDistance; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                farPlaneDistance = value;
            }
        }

        public DofSettings()
        {
            Blur = new BlurSettings
            {
                Enabled = true,
                Amount = 2
            };
        }
    }
}
