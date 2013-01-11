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
    /// スクリーン スペース シャドウ マッピング (Screen Space Shadow Mapping) を行うためのクラスです。
    /// </summary>
    public sealed class Sssm : PostProcessor, IDisposable
    {
        #region Settings

        public sealed class Settings
        {
            float mapScale = 0.25f;

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

            public Settings()
            {
                Blur = new BlurSettings();
            }
        }

        #endregion

        #region ShadowSceneEffect

        sealed class ShadowSceneEffect : Effect, IEffectMatrices
        {
            //====================================================================
            // EffectParameter

            EffectParameter projection;

            EffectParameter view;

            EffectParameter world;

            EffectParameter depthBias;

            EffectParameter splitCount;

            EffectParameter splitDistances;

            EffectParameter splitLightViewProjections;

            EffectParameter[] shadowMaps;

            //--------------------------------------------------------------------
            // Classic specific

            EffectParameter shadowMapSize;

            EffectParameter shadowMapTexelSize;

            //--------------------------------------------------------------------
            // PCF specific

            EffectParameter pcfOffsetsParameter;

            //====================================================================
            // EffectTechnique

            ShadowMap.Techniques shadowMapTechnique;

            EffectTechnique classicTechnique;

            EffectTechnique pcf2x2Technique;

            EffectTechnique pcf3x3Technique;

            EffectTechnique vsmTechnique;

            // I/F
            public Matrix Projection
            {
                get { return projection.GetValueMatrix(); }
                set { projection.SetValue(value); }
            }

            // I/F
            public Matrix View
            {
                get { return view.GetValueMatrix(); }
                set { view.SetValue(value); }
            }

            // I/F
            public Matrix World
            {
                get { return world.GetValueMatrix(); }
                set { world.SetValue(value); }
            }

            public float DepthBias
            {
                get { return depthBias.GetValueSingle(); }
                set { depthBias.SetValue(value); }
            }

            public int SplitCount
            {
                get { return splitCount.GetValueInt32(); }
                set { splitCount.SetValue(value); }
            }

            public float[] SplitDistances
            {
                get { return splitDistances.GetValueSingleArray(ShadowMap.Settings.MaxSplitCount); }
                set { splitDistances.SetValue(value); }
            }

            public Matrix[] SplitLightViewProjections
            {
                get { return splitLightViewProjections.GetValueMatrixArray(ShadowMap.Settings.MaxSplitCount); }
                set { splitLightViewProjections.SetValue(value); }
            }

            Texture2D[] shadowMapBuffer = new Texture2D[ShadowMap.Settings.MaxSplitCount];

            public Texture2D[] SplitShadowMaps
            {
                get
                {
                    for (int i = 0; i < ShadowMap.Settings.MaxSplitCount; i++)
                        shadowMapBuffer[i] = shadowMaps[i].GetValueTexture2D();
                    return shadowMapBuffer;
                }
                set
                {
                    if (value == null) return;

                    for (int i = 0; i < value.Length; i++)
                        shadowMaps[i].SetValue(value[i]);
                }
            }

            //--------------------------------------------------------------------
            // Classic & PCF specific

            // PCF の場合、PCF テクニックを設定する前に必ず設定していなければならない。
            public int ShadowMapSize
            {
                get { return shadowMapSize.GetValueInt32(); }
                set
                {
                    if (value < 1) throw new ArgumentOutOfRangeException("value");

                    shadowMapSize.SetValue(value);
                    shadowMapTexelSize.SetValue(1 / (float) value);
                }
            }

            //
            //--------------------------------------------------------------------

            public ShadowMap.Techniques ShadowMapTechnique
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
                        case ShadowMap.Techniques.Pcf2x2:
                            CurrentTechnique = pcf2x2Technique;
                            InitializePcfKernel(2);
                            break;
                        case ShadowMap.Techniques.Pcf3x3:
                            CurrentTechnique = pcf3x3Technique;
                            InitializePcfKernel(3);
                            break;
                        default:
                            CurrentTechnique = classicTechnique;
                            break;
                    }
                }
            }

            public ShadowSceneEffect(Effect cloneSource)
                : base(cloneSource)
            {
                world = Parameters["World"];
                view = Parameters["View"];
                projection = Parameters["Projection"];

                depthBias = Parameters["DepthBias"];
                splitCount = Parameters["SplitCount"];
                splitDistances = Parameters["SplitDistances"];
                splitLightViewProjections = Parameters["SplitLightViewProjections"];

                shadowMaps = new EffectParameter[ShadowMap.Settings.MaxSplitCount];
                for (int i = 0; i < shadowMaps.Length; i++)
                    shadowMaps[i] = Parameters["ShadowMap" + i];

                shadowMapSize = Parameters["ShadowMapSize"];
                shadowMapTexelSize = Parameters["ShadowMapTexelSize"];

                pcfOffsetsParameter = Parameters["PcfOffsets"];

                classicTechnique = Techniques["Classic"];
                pcf2x2Technique = Techniques["Pcf2x2"];
                pcf3x3Technique = Techniques["Pcf3x3"];
                vsmTechnique = Techniques["Vsm"];

                ShadowMapTechnique = ShadowMap.DefaultShadowMapTechnique;
            }

            //====================================================================
            // PCF specific

            void InitializePcfKernel(int kernelSize)
            {
                var texelSize = shadowMapTexelSize.GetValueSingle();

                int start;
                if (kernelSize % 2 == 0)
                {
                    start = -(kernelSize / 2) + 1;
                }
                else
                {
                    start = -(kernelSize - 1) / 2;
                }
                var end = start + kernelSize;

                var tapCount = kernelSize * kernelSize;
                var offsets = new Vector2[tapCount];

                int i = 0;
                for (int y = start; y < end; y++)
                {
                    for (int x = start; x < end; x++)
                    {
                        offsets[i++] = new Vector2(x, y) * texelSize;
                    }
                }

                pcfOffsetsParameter.SetValue(offsets);
            }

            //
            //====================================================================
        }

        #endregion

        #region SssmMonitor

        public sealed class SssmMonitor : PostProcessorMonitor
        {
            public event EventHandler BeginDrawShadowScene = delegate { };

            public event EventHandler EndDrawShadowScene = delegate { };

            public event EventHandler BeginFilter = delegate { };

            public event EventHandler EndFilter = delegate { };

            internal SssmMonitor(Sssm sssm) : base(sssm) { }

            internal void OnBeginDrawShadowScene()
            {
                BeginDrawShadowScene(PostProcessor, EventArgs.Empty);
            }

            internal void OnEndDrawShadowScene()
            {
                EndDrawShadowScene(PostProcessor, EventArgs.Empty);
            }

            internal void OnBeginFilter()
            {
                BeginFilter(PostProcessor, EventArgs.Empty);
            }

            internal void OnEndFilter()
            {
                EndFilter(PostProcessor, EventArgs.Empty);
            }
        }

        #endregion

        ShadowSceneEffect shadowSceneEffect;

        Effect sssmEffect;

        EffectParameter shadowColorParameter;
        
        EffectParameter shadowSceneMapParameter;

        EffectPass currentPass;

        GaussianBlur blur;

        RenderTarget2D shadowSceneMap;

        ShadowMap.Settings shadowMapSettings;

        Settings sssmSettings;

        Vector3 shadowColor;

        public Vector3 ShadowColor
        {
            get { return shadowColor; }
            set { shadowColor = value; }
        }

        public SssmMonitor Monitor { get; private set; }

        public Sssm(SpriteBatch spriteBatch, ShadowMap.Settings shadowMapSettings, Settings sssmSettings,
            Effect shadowSceneEffect, Effect sssmEffect, Effect blurEffect)
            : base(spriteBatch)
        {
            if (shadowMapSettings == null) throw new ArgumentNullException("shadowMapSettings");
            if (sssmSettings == null) throw new ArgumentNullException("sssmSettings");
            if (shadowSceneEffect == null) throw new ArgumentNullException("shadowSceneEffect");
            if (sssmEffect == null) throw new ArgumentNullException("sssmEffect");
            if (blurEffect == null) throw new ArgumentNullException("blurEffect");

            this.shadowMapSettings = shadowMapSettings;
            this.sssmSettings = sssmSettings;
            this.sssmEffect = sssmEffect;

            //================================================================
            // シャドウ シーン

            //----------------------------------------------------------------
            // エフェクト

            this.shadowSceneEffect = new ShadowSceneEffect(shadowSceneEffect);
            this.shadowSceneEffect.DepthBias = shadowMapSettings.DepthBias;
            this.shadowSceneEffect.SplitCount = shadowMapSettings.SplitCount;
            this.shadowSceneEffect.ShadowMapSize = shadowMapSettings.Size;
            this.shadowSceneEffect.ShadowMapTechnique = shadowMapSettings.Technique;

            //----------------------------------------------------------------
            // レンダ ターゲット

            var pp = GraphicsDevice.PresentationParameters;
            var width = (int) (pp.BackBufferWidth * sssmSettings.MapScale);
            var height = (int) (pp.BackBufferHeight * sssmSettings.MapScale);

            // メモ: ブラーをかける場合があるので RenderTargetUsage.PreserveContents で作成。
            shadowSceneMap = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

            //================================================================
            // スクリーン スペース シャドウ マッピング

            //----------------------------------------------------------------
            // エフェクト

            shadowColorParameter = sssmEffect.Parameters["ShadowColor"];
            shadowSceneMapParameter = sssmEffect.Parameters["ShadowSceneMap"];
            currentPass = sssmEffect.CurrentTechnique.Passes[0];

            //----------------------------------------------------------------
            // ブラー

            if (sssmSettings.BlurEnabled)
            {
                var blurSettings = sssmSettings.Blur;
                blur = new GaussianBlur(blurEffect, spriteBatch, width, height, SurfaceFormat.Vector2,
                    blurSettings.Radius, blurSettings.Amount);
            }

            //================================================================
            // モニタ

            Monitor = new SssmMonitor(this);
        }

        public override void Process(IPostProcessorContext context)
        {
            Monitor.OnBeginProcess();

            DrawShadowScene(context);
            Filter(context);

            if (DebugMapDisplay.Available) DebugMapDisplay.Instance.Add(shadowSceneMap);

            Monitor.OnEndProcess();
        }

        void DrawShadowScene(IPostProcessorContext context)
        {
            Monitor.OnBeginDrawShadowScene();

            var camera = context.ActiveCamera;
            var shadowMap = context.ShadowMap;
            var visibleSceneObjects = context.VisibleSceneObjects;

            //----------------------------------------------------------------
            // エフェクト

            shadowSceneEffect.View = camera.View.Matrix;
            shadowSceneEffect.Projection = camera.Projection.Matrix;
            shadowSceneEffect.SplitDistances = shadowMap.SplitDistances;
            shadowSceneEffect.SplitLightViewProjections = shadowMap.SplitLightViewProjections;
            shadowSceneEffect.SplitShadowMaps = shadowMap.SplitShadowMaps;

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            GraphicsDevice.SetRenderTarget(shadowSceneMap);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

            foreach (var sceneObject in visibleSceneObjects)
                sceneObject.Draw(shadowSceneEffect);

            GraphicsDevice.SetRenderTarget(null);

            Monitor.OnEndDrawShadowScene();
        }

        void Filter(IPostProcessorContext context)
        {
            Monitor.OnBeginFilter();

            if (blur != null) blur.Filter(shadowSceneMap);

            //----------------------------------------------------------------
            // エフェクト

            shadowColorParameter.SetValue(shadowColor);
            shadowSceneMapParameter.SetValue(shadowSceneMap);

            //----------------------------------------------------------------
            // 描画

            GraphicsDevice.SetRenderTarget(context.Destination);

            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, sssmEffect);
            SpriteBatch.Draw(context.Source, context.Destination.Bounds, Color.White);
            SpriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            Monitor.OnEndFilter();
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~Sssm()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                shadowSceneEffect.Dispose();
                shadowSceneMap.Dispose();
                if (blur != null) blur.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
