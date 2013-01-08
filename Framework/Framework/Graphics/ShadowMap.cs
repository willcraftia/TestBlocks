#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    /// <summary>
    /// シャドウ マップを生成および管理するクラスです。
    /// このクラスは、PSSM (parallel split shadow maps) によりシャドウ マップを生成します。
    /// PSSM によるシャドウ マップの分割を必要としない場合には、分割数を 1 として利用します。
    /// </summary>
    public sealed class ShadowMap : IDisposable
    {
        #region Techniques

        public enum Techniques
        {
            /// <summary>
            /// クラシック。
            /// </summary>
            Classic,

            /// <summary>
            /// VSM (Variant Shadow Mapping)。
            /// </summary>
            Vsm,

            /// <summary>
            /// PCF (Percentage Closer Filtering) 2x2 カーネル。
            /// </summary>
            Pcf2x2,

            /// <summary>
            /// PCF (Percentage Closer Filtering) 3x3 カーネル。
            /// </summary>
            Pcf3x3
        }

        #endregion

        #region Settings

        public sealed class Settings
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
            // 最も無難な設定が Classic であり、ライト ブリーディングを解決できるならば VSM。
            //

            public const SurfaceFormat DefaultFormat = SurfaceFormat.Single;

            public const ShadowMap.Techniques DefaultTechnique = ShadowMap.Techniques.Classic;

            // シャドウ マップのサイズに従って適切な値が変わるので注意。
            // シャドウ マップ サイズを小さくすると、より大きな深度バイアスが必要。
            public const float DefaultDepthBias = 0.0005f;

            public const int MinSplitCount = 1;

            public const int MaxSplitCount = 3;

            public const int DefaultSplitCount = 3;

            public const float DefaultSplitLambda = 0.5f;

            ShadowMap.Techniques technique = DefaultTechnique;

            int size = DefaultSize;

            SurfaceFormat format = DefaultFormat;

            float depthBias = DefaultDepthBias;

            float nearPlaneDistance = PerspectiveFov.DefaultNearPlaneDistance;

            float farPlaneDistance = PerspectiveFov.DefaultFarPlaneDistance;

            int splitCount = DefaultSplitCount;

            float splitLambda = DefaultSplitLambda;

            /// <summary>
            /// シャドウ マップ生成方法の種類を取得または設定します。
            /// </summary>
            public ShadowMap.Techniques Technique
            {
                get { return technique; }
                set
                {
                    technique = value;

                    switch (technique)
                    {
                        case ShadowMap.Techniques.Vsm:
                            format = SurfaceFormat.Vector2;
                            break;
                        default:
                            format = SurfaceFormat.Single;
                            break;
                    }
                }
            }

            /// <summary>
            /// シャドウ マップのサイズを取得または設定します。
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
            /// シャドウ マップの SurfaceFormat を取得または設定します。
            /// </summary>
            public SurfaceFormat Format
            {
                get { return format; }
            }

            /// <summary>
            /// シャドウ マップの深度バイアスを取得または設定します。
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
            /// シャドウ マップ描画で使用するカメラの NearPlaneDistance を取得または設定します。
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
            /// シャドウ マップ描画で使用するカメラの FarPlaneDistance を取得または設定します。
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
            /// シャドウ マップ分割数を取得または設定します。
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
            /// シャドウ マップ分割ラムダ値を取得または設定します。
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
            /// 分散シャドウ マップのブラー設定を取得します。
            /// </summary>
            public BlurSettings VsmBlur { get; private set; }

            public Settings()
            {
                VsmBlur = new BlurSettings();
            }
        }

        #endregion

        #region ShadowMapEffect

        sealed class ShadowMapEffect : Effect, IEffectMatrices
        {
            //====================================================================
            // EffectParameter

            EffectParameter world;

            EffectParameter lightViewProjection;

            //====================================================================
            // EffectTechnique

            Techniques shadowMapTechnique;

            EffectTechnique defaultTechnique;

            EffectTechnique vsmTechnique;

            // I/F
            public Matrix Projection
            {
                get { return Matrix.Identity; }
                set { }
            }

            // I/F
            public Matrix View
            {
                get { return Matrix.Identity; }
                set { }
            }

            // I/F
            public Matrix World
            {
                get { return world.GetValueMatrix(); }
                set { world.SetValue(value); }
            }

            public Matrix LightViewProjection
            {
                get { return lightViewProjection.GetValueMatrix(); }
                set { lightViewProjection.SetValue(value); }
            }

            public Techniques ShadowMapTechnique
            {
                get { return shadowMapTechnique; }
                set
                {
                    shadowMapTechnique = value;

                    switch (shadowMapTechnique)
                    {
                        case ShadowMap.Techniques.Vsm:
                            CurrentTechnique = vsmTechnique;
                            break;
                        default:
                            CurrentTechnique = defaultTechnique;
                            break;
                    }
                }
            }

            public ShadowMapEffect(Effect cloneSource)
                : base(cloneSource)
            {
                world = Parameters["World"];
                lightViewProjection = Parameters["LightViewProjection"];

                defaultTechnique = Techniques["Default"];
                vsmTechnique = Techniques["Vsm"];

                ShadowMapTechnique = DefaultShadowMapTechnique;
            }
        }

        #endregion

        #region ShadowMapMonitor

        public sealed class ShadowMapMonitor
        {
            #region Split

            public sealed class Split
            {
                public int ShadowCasterCount { get; set; }

                internal Split() { }
            }

            #endregion

            Split[] split;

            public int SplitCount { get; private set; }

            public Split this[int index]
            {
                get
                {
                    if (index < 0 || split.Length <= index) throw new ArgumentOutOfRangeException("index");
                    return split[index];
                }
            }

            public int TotalShadowCasterCount { get; internal set; }

            internal ShadowMapMonitor(int splitCount)
            {
                SplitCount = splitCount;

                split = new Split[SplitCount];
                for (int i = 0; i < split.Length; i++) split[i] = new Split();
            }
        }

        #endregion

        public const Techniques DefaultShadowMapTechnique = Techniques.Classic;

        BasicCamera internalCamera = new BasicCamera("ShadowMapInternal");

        Vector3[] corners = new Vector3[8];

        float inverseSplitCount;

        BasicCamera[] splitCameras;

        float[] splitDistances;

        float[] safeSplitDistances;

        LightCamera[] splitLightCameras;

        Matrix[] safeSplitLightViewProjections;

        RenderTarget2D[] splitRenderTargets;

        Texture2D[] safeSplitShadowMaps;

        Queue<ShadowCaster>[] splitShadowCasters;

        ShadowMapEffect shadowMapEffect;

        SpriteBatch spriteBatch;

        Blur blur;

        BoundingBox frustumBoundingBox;

        Settings settings;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public Techniques Technique { get; private set; }

        public int Size { get; private set; }

        public float DepthBias { get; private set; }

        public int SplitCount { get; private set; }

        public float[] SplitDistances
        {
            get
            {
                Array.Copy(splitDistances, safeSplitDistances, splitDistances.Length);
                return safeSplitDistances;
            }
        }

        public Matrix[] SplitLightViewProjections
        {
            get
            {
                for (int i = 0; i < splitLightCameras.Length; i++)
                    safeSplitLightViewProjections[i] = splitLightCameras[i].LightViewProjection;
                return safeSplitLightViewProjections;
            }
        }

        public Texture2D[] SplitShadowMaps
        {
            get
            {
                for (int i = 0; i < safeSplitShadowMaps.Length; i++)
                    safeSplitShadowMaps[i] = splitRenderTargets[i];
                return safeSplitShadowMaps;
            }
        }

        public ShadowMapMonitor Monitor;

        public ShadowMap(GraphicsDevice graphicsDevice, Settings settings,
            SpriteBatch spriteBatch, Effect shadowMapEffect, Effect blurEffect)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (settings == null) throw new ArgumentNullException("settings");
            if (spriteBatch == null) throw new ArgumentNullException("spriteBatch");
            if (shadowMapEffect == null) throw new ArgumentNullException("shadowMapEffect");
            if (blurEffect == null) throw new ArgumentNullException("blurEffect");

            GraphicsDevice = graphicsDevice;
            this.settings = settings;
            this.spriteBatch = spriteBatch;
            this.shadowMapEffect = new ShadowMapEffect(shadowMapEffect);
            this.shadowMapEffect.ShadowMapTechnique = settings.Technique;

            Technique = settings.Technique;
            Size = settings.Size;
            DepthBias = settings.DepthBias;
            SplitCount = settings.SplitCount;
            inverseSplitCount = 1 / (float) SplitCount;

            splitDistances = new float[SplitCount + 1];
            safeSplitDistances = new float[SplitCount + 1];
            safeSplitLightViewProjections = new Matrix[SplitCount];
            safeSplitShadowMaps = new Texture2D[SplitCount];

            splitCameras = new BasicCamera[SplitCount];
            for (int i = 0; i < splitCameras.Length; i++)
                splitCameras[i] = new BasicCamera("PssmLight" + i);

            splitLightCameras = new LightCamera[SplitCount];
            for (int i = 0; i < splitLightCameras.Length; i++)
                splitLightCameras[i] = new LightCamera(settings.Size);

            // TODO: パラメータ見直し or 外部設定化。
            var pp = GraphicsDevice.PresentationParameters;
            // メモ: ブラーをかける場合があるので RenderTargetUsage.PreserveContents で作成。
            splitRenderTargets = new RenderTarget2D[SplitCount];
            for (int i = 0; i < splitRenderTargets.Length; i++)
            {
                splitRenderTargets[i] = new RenderTarget2D(GraphicsDevice, settings.Size, settings.Size,
                    false, settings.Format, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
                splitRenderTargets[i].Name = "ShadowMap" + i;
            }

            // TODO: 初期容量。
            splitShadowCasters = new Queue<ShadowCaster>[SplitCount];
            for (int i = 0; i < splitShadowCasters.Length; i++)
                splitShadowCasters[i] = new Queue<ShadowCaster>();

            if (settings.Technique == Techniques.Vsm)
            {
                blur = new Blur(blurEffect, spriteBatch, settings.Size, settings.Size, SurfaceFormat.Vector2,
                    settings.VsmBlur.Radius, settings.VsmBlur.Amount);
            }

            Monitor = new ShadowMapMonitor(SplitCount);
        }

        public void Prepare(ICamera viewerCamera)
        {
            if (viewerCamera == null) throw new ArgumentNullException("viewerCamera");

            internalCamera.View.Position = viewerCamera.View.Position;
            internalCamera.View.Direction = viewerCamera.View.Direction;
            internalCamera.View.Up = viewerCamera.View.Up;
            internalCamera.Projection.Fov = viewerCamera.Projection.Fov;
            internalCamera.Projection.AspectRatio = viewerCamera.Projection.AspectRatio;
            internalCamera.Projection.NearPlaneDistance = settings.NearPlaneDistance;
            internalCamera.Projection.FarPlaneDistance = settings.FarPlaneDistance;
            internalCamera.Update();

            PrepareSplitCameras();
        }

        public void Prepare(ICamera viewerCamera, ref BoundingBox sceneBoundingBox)
        {
            if (viewerCamera == null) throw new ArgumentNullException("viewerCamera");

            internalCamera.View.Position = viewerCamera.View.Position;
            internalCamera.View.Direction = viewerCamera.View.Direction;
            internalCamera.View.Up = viewerCamera.View.Up;
            internalCamera.Projection.Fov = viewerCamera.Projection.Fov;
            internalCamera.Projection.AspectRatio = viewerCamera.Projection.AspectRatio;
            internalCamera.Projection.NearPlaneDistance = settings.NearPlaneDistance;
            internalCamera.Projection.FarPlaneDistance = settings.FarPlaneDistance;
            internalCamera.Update();

            PrepareSplitCameras(ref sceneBoundingBox);
        }

        void PrepareSplitCameras()
        {
            // 視錐台を含む AABB をシーン領域のデフォルトとしておく。
            internalCamera.Frustum.GetCorners(corners);
            var sceneBoundingBox = BoundingBox.CreateFromPoints(corners);

            PrepareSplitCameras(ref sceneBoundingBox);
        }

        void PrepareSplitCameras(ref BoundingBox sceneBoundingBox)
        {
            internalCamera.Frustum.GetCorners(corners);
            frustumBoundingBox = BoundingBox.CreateFromPoints(corners);

            var far = CalculateFarPlaneDistance(internalCamera, ref sceneBoundingBox);
            CalculateSplitDistances(internalCamera, far);

            for (int i = 0; i < SplitCount; i++)
            {
                splitCameras[i].View.Position = internalCamera.View.Position;
                splitCameras[i].View.Direction = internalCamera.View.Direction;
                splitCameras[i].View.Up = internalCamera.View.Up;
                splitCameras[i].Projection.Fov = internalCamera.Projection.Fov;
                splitCameras[i].Projection.AspectRatio = internalCamera.Projection.AspectRatio;
                splitCameras[i].Projection.NearPlaneDistance = splitDistances[i];
                splitCameras[i].Projection.FarPlaneDistance = splitDistances[i + 1];
                splitCameras[i].Update();

                Monitor[i].ShadowCasterCount = 0;
            }

            Monitor.TotalShadowCasterCount = 0;
        }

        public void TryAddShadowCaster(ShadowCaster shadowCaster)
        {
            if (!shadowCaster.BoundingSphere.Intersects(frustumBoundingBox)) return;
            if (!shadowCaster.BoundingBox.Intersects(frustumBoundingBox)) return;

            for (int i = 0; i < splitCameras.Length; i++)
            {
                var lightCamera = splitCameras[i];

                bool shouldAdd = false;
                if (shadowCaster.BoundingSphere.Intersects(lightCamera.Frustum))
                {
                    shouldAdd = true;
                }
                else if (shadowCaster.BoundingBox.Intersects(lightCamera.Frustum))
                {
                    shouldAdd = true;
                }

                if (shouldAdd)
                {
                    // 投影オブジェクトとして登録。
                    splitShadowCasters[i].Enqueue(shadowCaster);

                    // AABB の頂点を包含座標として登録。
                    // TODO:
                    // これをやると領域が広くなりすぎてカメラが遠方に移動してしまい、
                    // PSSM の恩恵が得られなくなる。
                    //splitLightCameras[i].AddLightVolumePoints(corners);

                    Monitor[i].ShadowCasterCount++;
                    Monitor.TotalShadowCasterCount++;

                    //break;
                }
            }
        }

        // シャドウ マップを描画。
        public void Draw(ref Vector3 lightDirection)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            // 各ライト カメラで描画。
            for (int i = 0; i < splitCameras.Length; i++)
            {
                var camera = splitCameras[i];
                var renderTarget = splitRenderTargets[i];
                var shadowCasters = splitShadowCasters[i];

                //------------------------------------------------------------
                // ライトのビュー×射影行列の更新

                splitLightCameras[i].Update(camera, ref lightDirection);

                //------------------------------------------------------------
                // エフェクト

                shadowMapEffect.LightViewProjection = splitLightCameras[i].LightViewProjection;

                //------------------------------------------------------------
                // 描画

                GraphicsDevice.SetRenderTarget(renderTarget);
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

                while (0 < shadowCasters.Count)
                {
                    var shadowCaster = shadowCasters.Dequeue();
                    shadowCaster.Draw(shadowMapEffect);
                }

                if (shadowMapEffect.ShadowMapTechnique == Techniques.Vsm && blur != null)
                    blur.Filter(renderTarget);

                GraphicsDevice.SetRenderTarget(null);

                if (DebugMapDisplay.Available) DebugMapDisplay.Instance.Add(renderTarget);
            }
        }

        public Texture2D GetShadowMap(int index)
        {
            if (index < 0 || splitRenderTargets.Length < index) throw new ArgumentOutOfRangeException("index");

            return splitRenderTargets[index];
        }

        float CalculateFarPlaneDistance(ICamera camera, ref BoundingBox sceneBoundingBox)
        {
            var viewMatrix = camera.View.Matrix;

            // 領域の最も遠い点を探す。
            // z = 0 は視点。
            // より小さな z がより遠い点。
            var maxFar = 0.0f;
            sceneBoundingBox.GetCorners(corners);
            for (int i = 0; i < 8; i++)
            {
                // ビュー座標へ変換。
                var z =
                    corners[i].X * viewMatrix.M13 +
                    corners[i].Y * viewMatrix.M23 +
                    corners[i].Z * viewMatrix.M33 +
                    viewMatrix.M43;

                if (z < maxFar) maxFar = z;
            }

            // 見つかった最も遠い点の z で farPlaneDistance を決定。
            return camera.Projection.NearPlaneDistance - maxFar;
        }

        void CalculateSplitDistances(ICamera camera, float farPlaneDistance)
        {
            var near = camera.Projection.NearPlaneDistance;
            var far = farPlaneDistance;
            var farNearRatio = far / near;
            var splitLambda = settings.SplitLambda;

            for (int i = 0; i < splitDistances.Length; i++)
            {
                float idm = i * inverseSplitCount;

                // CL = n * (f / n)^(i / m)
                float log = (float) (near * Math.Pow(farNearRatio, idm));

                // CU = n + (f - n) * (i / m)
                float uniform = near + (far - near) * idm;
                // REFERENCE: the version (?) in some actual codes,
                //float uniform = (near + idm) * (far - near);

                // C = CL * lambda + CU * (1 - lambda)
                splitDistances[i] = log * splitLambda + uniform * (1.0f - splitLambda);
            }

            splitDistances[0] = near;
            splitDistances[splitDistances.Length - 1] = farPlaneDistance;
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~ShadowMap()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                shadowMapEffect.Dispose();
                if (blur != null) blur.Dispose();

                foreach (var splitRenderTarget in splitRenderTargets)
                    splitRenderTarget.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
