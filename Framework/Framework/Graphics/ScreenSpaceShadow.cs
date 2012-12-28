#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public abstract class ScreenSpaceShadow
    {
        EffectParameter shadowColorParameter;
        
        EffectParameter shadowSceneMapParameter;

        GaussianBlur gaussianBlur;

        float shadowSceneMapScale;

        Vector3 shadowColor;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public Effect Effect { get; set; }

        public SpriteBatch SpriteBatch { get; set; }

        public int ShadowMapSize { get; set; }

        public SurfaceFormat ShadowMapFormat { get; set; }

        public float ShadowSceneMapScale
        {
            get { return shadowSceneMapScale; }
            set
            {
                if (value <= 0 || 1 < value) throw new ArgumentOutOfRangeException("value");

                shadowSceneMapScale = value;
            }
        }

        public Vector3 ShadowColor
        {
            get { return shadowColor; }
            set { shadowColor = value; }
        }

        public bool BlurEnabled { get; set; }

        public int BlurRadius { get; set; }

        public float BlurAmount { get; set; }

        public Effect GaussianBlurEffect { get; set; }

        protected RenderTarget2D ShadowSceneMap { get; private set; }

        protected ScreenSpaceShadow(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            GraphicsDevice = graphicsDevice;
        }

        public void Initialize()
        {
            if (Effect == null) throw new InvalidOperationException("Effect is null.");
            if (SpriteBatch == null) throw new InvalidOperationException("SpriteBatch is null.");
            if (GaussianBlurEffect == null) throw new InvalidOperationException("GaussianBlurEffect is null.");

            //----------------------------------------------------------------
            // Effect

            shadowColorParameter = Effect.Parameters["ShadowColor"];
            shadowSceneMapParameter = Effect.Parameters["ShadowSceneMap"];

            //----------------------------------------------------------------
            // ShadowSceneMap (RenderTarget)

            var pp = GraphicsDevice.PresentationParameters;
            var width = (int) (pp.BackBufferWidth * ShadowSceneMapScale);
            var height = (int) (pp.BackBufferHeight * ShadowSceneMapScale);

            ShadowSceneMap = new RenderTarget2D(GraphicsDevice, width, height,
                false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PlatformContents);

            //----------------------------------------------------------------
            // GaussianBlur

            gaussianBlur = new GaussianBlur(GaussianBlurEffect, SpriteBatch, width, height, SurfaceFormat.Color, BlurRadius, BlurAmount);

            InitializeOverride();
        }

        public abstract void Prepare(View eyeView, PerspectiveFov eyeProjection);

        public void Draw(View eyeView, PerspectiveFov eyeProjection)
        {
            DrawShadowMap(eyeView, eyeProjection);
            DrawShadowSceneMap(eyeView, eyeProjection);
        }

        public void Filter(Texture2D source, RenderTarget2D destination)
        {
            if (ShadowSceneMap == null) throw new InvalidOperationException("ShadowSceneMap is null.");

            shadowSceneMapParameter.SetValue(ShadowSceneMap);
            shadowColorParameter.SetValue(shadowColor);

            var bounds = destination.GetBounds();
            var samplerState = destination.GetPreferredSamplerState();

            GraphicsDevice.SetRenderTarget(destination);
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, samplerState, null, null, Effect);
            SpriteBatch.Draw(source, bounds, Color.White);
            SpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
        }

        protected virtual void InitializeOverride() { }

        protected abstract void DrawShadowMap(View eyeView, PerspectiveFov eyeProjection);

        protected abstract void DrawShadowSceneMap(View eyeView, PerspectiveFov eyeProjection);

        protected void BlurShadowSceneMap()
        {
            if (!BlurEnabled) return;

            gaussianBlur.Filter(ShadowSceneMap, ShadowSceneMap);
        }
    }
}
