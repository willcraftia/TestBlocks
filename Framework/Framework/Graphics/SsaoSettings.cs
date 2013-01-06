#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class SsaoSettings
    {
        public const bool DefaultEnabled = true;

        public const float DefaultMapScale = 1;

        public const float DefaultTotalStrength = 1;

        public const float DefaultStrength = 1;

        public const float DefaultFalloff = 0.00001f;

        public const float DefaultRadius = 2;

        bool enabled = DefaultEnabled;

        float mapScale = DefaultMapScale;

        float farPlaneDistance = 64;

        float totalStrength = DefaultTotalStrength;
        
        float strength = DefaultStrength;
        
        float falloff = DefaultFalloff;
        
        float radius = DefaultRadius;

        /// <summary>
        /// スクリーン スペース アンビエント オクルージョンが有効か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (スクリーン スペース アンビエント オクルージョンが有効な場合)、false (それ以外の場合)。
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

        /// <summary>
        /// 法線深度マップ描画で使用するカメラの、遠くのビュー プレーンとの距離。
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

        public float TotalStrength
        {
            get { return totalStrength; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                totalStrength = value;
            }
        }

        public float Strength
        {
            get { return strength; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                strength = value;
            }
        }

        public float Falloff
        {
            get { return falloff; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                falloff = value;
            }
        }

        public float Radius
        {
            get { return radius; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                radius = value;
            }
        }

        public SsaoSettings()
        {
            Blur = new BlurSettings();
        }
    }
}
