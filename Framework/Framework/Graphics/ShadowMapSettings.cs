#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class ShadowMapSettings
    {
        //public const int DefaultSize = 512;
        //public const int DefaultSize = 1024;
        public const int DefaultSize = 2048;

        // メモ
        //
        // VSM が最も綺麗な影となるが、最前面以外の分割視錐台で深度テストが上手くいっていない。
        // また、高い崖のような地形による投影において、ライト ブリーディングが激しい。
        // なお、分割数 1 で VSM を行うと、カメラ近隣はほとんどが影なしと判定される。
        //
        // Pcf は、3x3 程度なら Classic とそれ程変わりがない。
        //

        public const SurfaceFormat DefaultFormat = SurfaceFormat.Vector2;

        public const ShadowMapTechniques DefaultTechnique = ShadowMapTechniques.Vsm;

        // シャドウ マップのサイズに従って適切な値が変わるので注意。
        // シャドウ マップ サイズを小さくすると、より大きな深度バイアスが必要。
        public const float DefaultDepthBias = 0.0005f;

        public const int MinSplitCount = 1;

        public const int MaxSplitCount = 3;

        public const int DefaultSplitCount = 3;

        public const float DefaultSplitLambda = 0.5f;

        ShadowMapTechniques technique = DefaultTechnique;

        int size = DefaultSize;

        SurfaceFormat format = DefaultFormat;

        float depthBias = DefaultDepthBias;

        float nearPlaneDistance = PerspectiveFov.DefaultNearPlaneDistance;

        float farPlaneDistance = PerspectiveFov.DefaultFarPlaneDistance;

        int splitCount = DefaultSplitCount;

        float splitLambda = DefaultSplitLambda;

        /// <summary>
        /// シャドウ マップ生成方法の種類。
        /// </summary>
        public ShadowMapTechniques Technique
        {
            get { return technique; }
            set
            {
                technique = value;

                switch (technique)
                {
                    case ShadowMapTechniques.Vsm:
                        format = SurfaceFormat.Vector2;
                        break;
                    default:
                        format = SurfaceFormat.Single;
                        break;
                }
            }
        }

        /// <summary>
        /// シャドウ マップのサイズ。
        /// </summary>
        public int Size
        {
            get { return size; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                size = value;
            }
        }

        /// <summary>
        /// シャドウ マップの SurfaceFormat。
        /// </summary>
        public SurfaceFormat Format
        {
            get { return format; }
        }

        /// <summary>
        /// シャドウ マップの深度バイアス。
        /// </summary>
        public float DepthBias
        {
            get { return depthBias; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                depthBias = value;
            }
        }

        /// <summary>
        /// シャドウ マップ描画で使用するカメラの、近くのビュー プレーンとの距離。
        /// </summary>
        public float NearPlaneDistance
        {
            get { return nearPlaneDistance; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                nearPlaneDistance = value;
            }
        }

        /// <summary>
        /// シャドウ マップ描画で使用するカメラの、遠くのビュー プレーンとの距離。
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

        /// <summary>
        /// シャドウ マップ分割数。
        /// </summary>
        public int SplitCount
        {
            get { return splitCount; }
            set
            {
                if (value < MinSplitCount || MaxSplitCount < value) throw new ArgumentOutOfRangeException("value");

                splitCount = value;
            }
        }

        /// <summary>
        /// シャドウ マップ分割ラムダ値。
        /// </summary>
        public float SplitLambda
        {
            get { return splitLambda; }
            set
            {
                if (value < 0 || 1 < value) throw new ArgumentOutOfRangeException("value");

                splitLambda = value;
            }
        }

        /// <summary>
        /// VSM 設定。
        /// </summary>
        public VsmSettings Vsm { get; private set; }

        public ShadowMapSettings()
        {
            Vsm = new VsmSettings();
        }
    }
}
