#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class Ssao : IDisposable
    {
        #region SsaoMapEffect

        sealed class SsaoMapEffect
        {
            EffectParameter totalStrength;
            
            EffectParameter strength;
            
            EffectParameter randomOffset;
            
            EffectParameter falloff;
            
            EffectParameter radius;

            EffectParameter randomNormalMap;
            
            EffectParameter normalDepthMap;
            
            public Effect Effect { get; private set; }

            public float TotalStrength
            {
                get { return totalStrength.GetValueSingle(); }
                set { totalStrength.SetValue(value); }
            }

            public float Strength
            {
                get { return strength.GetValueSingle(); }
                set { strength.SetValue(value); }
            }

            public float RandomOffset
            {
                get { return randomOffset.GetValueSingle(); }
                set { randomOffset.SetValue(value); }
            }

            public float Falloff
            {
                get { return falloff.GetValueSingle(); }
                set { falloff.SetValue(value); }
            }

            public float Radius
            {
                get { return radius.GetValueSingle(); }
                set { radius.SetValue(value); }
            }

            public Texture2D RandomNormalMap
            {
                get { return randomNormalMap.GetValueTexture2D(); }
                set { randomNormalMap.SetValue(value); }
            }

            public Texture2D NormalDepthMap
            {
                get { return normalDepthMap.GetValueTexture2D(); }
                set { normalDepthMap.SetValue(value); }
            }

            public SsaoMapEffect(Effect effect)
            {
                Effect = effect;

                totalStrength = effect.Parameters["TotalStrength"];
                strength = effect.Parameters["Strength"];
                randomOffset = effect.Parameters["RandomOffset"];
                falloff = effect.Parameters["Falloff"];
                radius = effect.Parameters["Radius"];
                randomNormalMap = effect.Parameters["RandomNormalMap"];
                normalDepthMap = effect.Parameters["NormalDepthMap"];
            }
        }

        #endregion

        #region SsaoMapBlurEffect

        sealed class SsaoMapBlurEffect
        {
            EffectParameter kernelSize;
            
            EffectParameter weights;
            
            EffectParameter offsetH;
            
            EffectParameter offsetV;
            
            EffectParameter normalDepthMap;

            EffectTechnique horizontalBlurTechnique;

            EffectTechnique verticalBlurTechnique;

            public Effect Effect { get; private set; }

            public int Width { get; set; }

            public int Height { get; set; }

            public int Radius { get; set; }

            public float Amount { get; set; }

            public Texture2D NormalDepthMap
            {
                get { return normalDepthMap.GetValueTexture2D(); }
                set { normalDepthMap.SetValue(value); }
            }

            public SsaoMapBlurEffect(Effect effect)
            {
                Effect = effect;

                kernelSize = effect.Parameters["KernelSize"];
                weights = effect.Parameters["Weights"];
                offsetH = effect.Parameters["OffsetsH"];
                offsetV = effect.Parameters["OffsetsV"];
                normalDepthMap = effect.Parameters["NormalDepthMap"];

                horizontalBlurTechnique = effect.Techniques["HorizontalBlur"];
                verticalBlurTechnique = effect.Techniques["VerticalBlur"];
            }

            public void Initialize()
            {
                kernelSize.SetValue(Radius * 2 + 1);
                PopulateWeights();
                PopulateOffsetsH();
                PopulateOffsetsV();
            }

            public void EnableHorizontalBlurTechnique()
            {
                Effect.CurrentTechnique = horizontalBlurTechnique;
            }

            public void EnableVerticalBlurTechnique()
            {
                Effect.CurrentTechnique = verticalBlurTechnique;
            }

            /// <summary>
            /// Calculates the kernel and populates them into the shader. 
            /// </summary>
            void PopulateWeights()
            {
                var w = new float[Radius * 2 + 1];
                var totalWeight = 0.0f;
                var sigma = Radius / Amount;

                int index = 0;
                for (int i = -Radius; i <= Radius; i++)
                {
                    w[index] = CalculateGaussian(sigma, i);
                    totalWeight += w[index];
                    index++;
                }

                // Normalize
                for (int i = 0; i < w.Length; i++)
                {
                    w[i] /= totalWeight;
                }

                weights.SetValue(w);
            }

            float CalculateGaussian(float sigma, float n)
            {
                var twoSigmaSquare = 2.0f * sigma * sigma;
                //
                // REFERENCE: (float) Math.Sqrt(2.0f * Math.PI * sigma * sigma)
                //
                var sigmaRoot = (float) Math.Sqrt(Math.PI * twoSigmaSquare);

                return (float) Math.Exp(-(n * n) / twoSigmaSquare) / sigmaRoot;
            }

            void PopulateOffsetsH()
            {
                offsetH.SetValue(CalculateOffsets(1.0f / (float) Width, 0));
            }

            void PopulateOffsetsV()
            {
                offsetV.SetValue(CalculateOffsets(0, 1.0f / (float) Height));
            }

            Vector2[] CalculateOffsets(float dx, float dy)
            {
                var offsets = new Vector2[Radius * 2 + 1];

                int index = 0;
                for (int i = -Radius; i <= Radius; i++)
                {
                    offsets[index] = new Vector2(i * dx, i * dy);
                    index++;
                }

                return offsets;
            }
        }

        #endregion

        #region SsaoEffect

        public sealed class SsaoEffect
        {
            EffectParameter shadowColor;
            
            EffectParameter ssaoMap;

            public Effect Effect { get; private set; }

            public Vector3 ShadowColor
            {
                get { return shadowColor.GetValueVector3(); }
                set { shadowColor.SetValue(value); }
            }

            public Texture2D SsaoMap
            {
                get { return ssaoMap.GetValueTexture2D(); }
                set { ssaoMap.SetValue(value); }
            }

            public SsaoEffect(Effect effect)
            {
                Effect = effect;

                shadowColor = effect.Parameters["ShadowColor"];
                ssaoMap = effect.Parameters["SsaoMap"];
            }
        }

        #endregion

        SpriteBatch spriteBatch;

        NormalDepthMapEffect normalDepthMapEffect;

        SsaoMapEffect ssaoMapEffect;

        SsaoMapBlurEffect ssaoMapBlurEffect;

        SsaoEffect ssaoEffect;

        BasicCamera internalCamera = new BasicCamera("EdgeInternal");

        Vector3[] frustumCorners = new Vector3[8];

        BoundingSphere frustumSphere;

        RenderTarget2D blurRenderTarget;

        FullscreenQuad fullscreenQuad;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public SsaoSettings Settings { get; private set; }

        public RenderTarget2D NormalDepthMap { get; private set; }

        public RenderTarget2D SsaoMap { get; private set; }

        public Ssao(GraphicsDevice graphicsDevice, SsaoSettings settings, SpriteBatch spriteBatch,
            Effect normalDepthMapEffect, Effect ssaoMapEffect, Effect ssaoMapBlurEffect, Effect ssaoEffect,
            Texture2D randomNormalMap)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (settings == null) throw new ArgumentNullException("settings");
            if (spriteBatch == null) throw new ArgumentNullException("spriteBatch");
            if (normalDepthMapEffect == null) throw new ArgumentNullException("normalDepthMapEffect");
            if (ssaoMapEffect == null) throw new ArgumentNullException("ssaoMapEffect");
            if (ssaoMapBlurEffect == null) throw new ArgumentNullException("ssaoMapBlurEffect");
            if (ssaoEffect == null) throw new ArgumentNullException("ssaoEffect");
            if (randomNormalMap == null) throw new ArgumentNullException("randomNormalMap");

            GraphicsDevice = graphicsDevice;
            Settings = settings;
            this.spriteBatch = spriteBatch;

            var pp = GraphicsDevice.PresentationParameters;
            var width = (int) (pp.BackBufferWidth * settings.MapScale);
            var height = (int) (pp.BackBufferHeight * settings.MapScale);

            //----------------------------------------------------------------
            // エフェクト

            // 法線深度マップ
            this.normalDepthMapEffect = new NormalDepthMapEffect(normalDepthMapEffect);
            
            // SSAO マップ
            this.ssaoMapEffect = new SsaoMapEffect(ssaoMapEffect);
            this.ssaoMapEffect.TotalStrength = settings.TotalStrength;
            this.ssaoMapEffect.Strength = settings.Strength;
            this.ssaoMapEffect.Falloff = settings.Falloff;
            this.ssaoMapEffect.Radius = settings.Radius;
            this.ssaoMapEffect.RandomNormalMap = randomNormalMap;

            // SSAO マップ ブラー
            this.ssaoMapBlurEffect = new SsaoMapBlurEffect(ssaoMapBlurEffect);
            this.ssaoMapBlurEffect.Width = width;
            this.ssaoMapBlurEffect.Height = height;
            this.ssaoMapBlurEffect.Radius = settings.Blur.Radius;
            this.ssaoMapBlurEffect.Amount = settings.Blur.Amount;
            this.ssaoMapBlurEffect.Initialize();

            // SSAO
            this.ssaoEffect = new SsaoEffect(ssaoEffect);

            //----------------------------------------------------------------
            // レンダ ターゲット

            // 法線深度マップ
            NormalDepthMap = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Vector4, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            // SSAO マップ
            SsaoMap = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

            // SSAO マップのブラー用
            blurRenderTarget = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

            //----------------------------------------------------------------
            // SsaoMap.fx は ps_3_0 を使うため、SpriteBatch を利用できない。
            // そこで、Quad を用いて SpriteBatch 風に SsaoMap.fx を利用する。

            fullscreenQuad = new FullscreenQuad(GraphicsDevice);
        }

        public void DrawSsaoMap(ICamera viewerCamera, IEnumerable<SceneObject> sceneObjects)
        {
            if (viewerCamera == null) throw new ArgumentNullException("viewerCamera");
            if (sceneObjects == null) throw new ArgumentNullException("sceneObjects");

            //================================================================
            //
            // 法線深度マップを描画
            //

            //----------------------------------------------------------------
            // 内部カメラの準備

            internalCamera.View.Position = viewerCamera.View.Position;
            internalCamera.View.Direction = viewerCamera.View.Direction;
            internalCamera.View.Up = viewerCamera.View.Up;
            internalCamera.Projection.Fov = viewerCamera.Projection.Fov;
            internalCamera.Projection.AspectRatio = viewerCamera.Projection.AspectRatio;
            internalCamera.Projection.NearPlaneDistance = viewerCamera.Projection.NearPlaneDistance;
            //internalCamera.Projection.FarPlaneDistance = Settings.FarPlaneDistance;
            internalCamera.Projection.FarPlaneDistance = viewerCamera.Projection.FarPlaneDistance;
            internalCamera.Update();

            internalCamera.Frustum.GetCorners(frustumCorners);
            frustumSphere = BoundingSphere.CreateFromPoints(frustumCorners);

            //----------------------------------------------------------------
            // エフェクト

            normalDepthMapEffect.View = internalCamera.View.Matrix;
            normalDepthMapEffect.Projection = internalCamera.Projection.Matrix;

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            GraphicsDevice.SetRenderTarget(NormalDepthMap);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

            foreach (var sceneObject in sceneObjects)
            {
                // 専用カメラの視錐台に含まれるもののみを描画。
                if (IsVisibleObject(sceneObject))
                    sceneObject.Draw(normalDepthMapEffect);
            }

            GraphicsDevice.SetRenderTarget(null);

            //================================================================
            //
            // SSAO マップを描画
            //

            //----------------------------------------------------------------
            // エフェクト

            ssaoMapEffect.RandomOffset = viewerCamera.View.Position.LengthSquared();
            ssaoMapEffect.NormalDepthMap = NormalDepthMap;

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.SetRenderTarget(SsaoMap);
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.Clear(Color.White);

            ssaoMapEffect.Effect.CurrentTechnique.Passes[0].Apply();
            fullscreenQuad.Draw();

            GraphicsDevice.SetRenderTarget(null);

            //================================================================
            //
            // SSAO マップへブラーを適用
            //

            //----------------------------------------------------------------
            // エフェクト

            ssaoMapBlurEffect.NormalDepthMap = NormalDepthMap;

            //----------------------------------------------------------------
            // HorizontalBlur テクニックで描画

            ssaoMapBlurEffect.EnableHorizontalBlurTechnique();

            GraphicsDevice.SetRenderTarget(blurRenderTarget);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, ssaoMapBlurEffect.Effect);
            spriteBatch.Draw(SsaoMap, blurRenderTarget.Bounds, Color.White);
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            //----------------------------------------------------------------
            // VerticalBlur テクニックで描画

            ssaoMapBlurEffect.EnableVerticalBlurTechnique();

            GraphicsDevice.SetRenderTarget(SsaoMap);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, ssaoMapBlurEffect.Effect);
            spriteBatch.Draw(blurRenderTarget, SsaoMap.Bounds, Color.White);
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
        }

        public void Filter(RenderTarget2D sceneMap, RenderTarget2D result)
        {
            if (sceneMap == null) throw new ArgumentNullException("sceneMap");
            if (result == null) throw new ArgumentNullException("result");

            //----------------------------------------------------------------
            // エフェクト

            ssaoEffect.SsaoMap = SsaoMap;

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.SetRenderTarget(result);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, ssaoEffect.Effect);
            spriteBatch.Draw(sceneMap, result.Bounds, Color.White);
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
        }

        bool IsVisibleObject(SceneObject sceneObject)
        {
            // 球同士で判定。
            if (!sceneObject.BoundingSphere.Intersects(frustumSphere)) return false;

            // 球と視錐台で判定。
            if (!sceneObject.BoundingSphere.Intersects(frustumSphere)) return false;

            // 視錐台と AABB で判定。
            return sceneObject.BoundingBox.Intersects(internalCamera.Frustum);
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~Ssao()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                normalDepthMapEffect.Dispose();
                NormalDepthMap.Dispose();
                SsaoMap.Dispose();
                blurRenderTarget.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
