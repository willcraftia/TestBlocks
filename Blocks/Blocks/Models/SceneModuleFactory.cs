#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class SceneModuleFactory : ISceneModuleFactory
    {
        ResourceManager resourceManager;
        
        AssetManager assetManager;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public SceneModuleFactory(GraphicsDevice graphicsDevice, ResourceManager resourceManager, AssetManager assetManager)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");
            if (assetManager == null) throw new ArgumentNullException("assetManager");

            GraphicsDevice = graphicsDevice;
            this.resourceManager = resourceManager;
            this.assetManager = assetManager;
        }

        // I/F
        public Effect CreateGaussianBlurEffect()
        {
            var resource = resourceManager.Load("content:Effects/GaussianBlur");
            return assetManager.Load<Effect>(resource);
        }

        // I/F
        public Effect CreateShadowMapEffect()
        {
            var resource = resourceManager.Load("content:Effects/ShadowMap");
            return assetManager.Load<Effect>(resource);
        }

        // I/F
        public Effect CreatePssmSceneEffect()
        {
            var resource = resourceManager.Load("content:Effects/PssmScene");
            return assetManager.Load<Effect>(resource);
        }

        public Effect CreateScreenSpaceShadowEffect()
        {
            var resource = resourceManager.Load("content:Effects/ScreenSpaceShadow");
            return assetManager.Load<Effect>(resource);
        }

        // I/F
        public Pssm CreatePssm(ShadowSettings shadowSettings)
        {
            return new Pssm(GraphicsDevice, shadowSettings, CreateGaussianBlurEffect());
        }

        // I/F
        public PssmScene CreatePssmScene(ShadowSettings shadowSettings)
        {
            return new PssmScene(GraphicsDevice, shadowSettings, CreatePssmSceneEffect());
        }

        // I/F
        public ScreenSpaceShadow CreateScreenSpaceShadow(ShadowSceneSettings shadowSceneSettings)
        {
            return new ScreenSpaceShadow(GraphicsDevice, shadowSceneSettings, CreateScreenSpaceShadowEffect(), CreateGaussianBlurEffect());
        }

        // I/F
        public BasicEffect CreateDebugBoundingBoxEffect()
        {
            return new BasicEffect(GraphicsDevice);
        }

        // I/F
        public BoundingBoxDrawer CreateDebugBoundingBoxDrawer()
        {
            return new BoundingBoxDrawer(GraphicsDevice);
        }
    }
}
