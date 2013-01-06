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

        SpriteBatch spriteBatch;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public SceneModuleFactory(GraphicsDevice graphicsDevice, ResourceManager resourceManager, AssetManager assetManager)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");
            if (assetManager == null) throw new ArgumentNullException("assetManager");

            GraphicsDevice = graphicsDevice;
            this.resourceManager = resourceManager;
            this.assetManager = assetManager;

            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        // I/F
        public ShadowMap CreateShadowMap(ShadowMapSettings shadowMapSettings)
        {
            var shadowMapEffectResource = resourceManager.Load("content:Effects/ShadowMap");
            var shadowMapEffect = assetManager.Load<Effect>(shadowMapEffectResource);

            return new ShadowMap(GraphicsDevice, shadowMapSettings, spriteBatch, shadowMapEffect, CreateGaussianBlurEffect());
        }

        // I/F
        public ShadowScene CreateShadowScene(ShadowSettings shadowSettings)
        {
            var effectResource = resourceManager.Load("content:Effects/ShadowScene");
            var effect = assetManager.Load<Effect>(effectResource);

            return new ShadowScene(GraphicsDevice, shadowSettings, effect);
        }

        // I/F
        public Sssm CreateSssm(SssmSettings sssmSettings)
        {
            var sssmEffectResource = resourceManager.Load("content:Effects/Sssm");
            var sssmEffect = assetManager.Load<Effect>(sssmEffectResource);

            return new Sssm(GraphicsDevice, sssmSettings, spriteBatch, sssmEffect, CreateGaussianBlurEffect());
        }

        // I/F
        public Dof CreateDof(DofSettings dofSettings)
        {
            var depthMapEffectResource = resourceManager.Load("content:Effects/DepthMap");
            var depthMapEffect = assetManager.Load<Effect>(depthMapEffectResource);

            var dofEffectResource = resourceManager.Load("content:Effects/Dof");
            var dofEffect = assetManager.Load<Effect>(dofEffectResource);

            return new Dof(GraphicsDevice, dofSettings, spriteBatch, depthMapEffect, dofEffect, CreateGaussianBlurEffect());
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

        Effect CreateGaussianBlurEffect()
        {
            var resource = resourceManager.Load("content:Effects/GaussianBlur");
            return assetManager.Load<Effect>(resource);
        }
    }
}
