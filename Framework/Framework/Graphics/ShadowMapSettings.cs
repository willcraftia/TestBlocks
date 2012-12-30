#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class ShadowMapSettings
    {
        public const int DefaultSize = 512;

        //public const SurfaceFormat DefaultFormat = SurfaceFormat.Vector2;
        public const SurfaceFormat DefaultFormat = SurfaceFormat.Color;

        public const ShadowMapTechniques DefaultTechnique = ShadowMapTechniques.Classic;

        public const float DefaultDepthBias = 0.005f;

        public const float DefaultFov = MathHelper.PiOver4;

        public const float DefaultAspectRatio = 1;

        public const float DefaultNearPlaneDistance = 0.1f;

        public const float DefaultFarPlaneDistance = 1000.0f;

        ShadowMapTechniques technique = DefaultTechnique;

        int size = DefaultSize;

        SurfaceFormat format = DefaultFormat;

        float depthBias = DefaultDepthBias;

        float fov = DefaultFov;

        float aspectRatio = DefaultAspectRatio;

        float nearPlaneDistance = DefaultNearPlaneDistance;
        
        float farPlaneDistance = DefaultFarPlaneDistance;

        /// <summary>
        /// シャドウ マッピングの種類。
        /// </summary>
        public ShadowMapTechniques Technique
        {
            get { return technique; }
            set { technique = value; }
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
            set { format = value; }
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
        /// シャドウ マップ描画で使用するライト カメラの y 方向の視野角 (ラジアン単位)。
        /// </summary>
        public float Fov
        {
            get { return fov; }
            set
            {
                if (value < 0 || MathHelper.Pi < value) throw new ArgumentOutOfRangeException("value");

                fov = value;
            }
        }

        /// <summary>
        /// シャドウ マップ描画で使用するライト カメラのアスペクト比。
        /// </summary>
        public float AspectRatio
        {
            get { return aspectRatio; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                aspectRatio = value;
            }
        }

        /// <summary>
        /// シャドウ マップ描画で使用するライト カメラの、近くのビュー プレーンとの距離。
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
        /// シャドウ マップ描画で使用するライト カメラの、遠くのビュー プレーンとの距離。
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
        /// VSM 設定。
        /// </summary>
        public VsmSettings Vsm { get; private set; }

        public ShadowMapSettings()
        {
            Vsm = new VsmSettings();
        }
    }
}
