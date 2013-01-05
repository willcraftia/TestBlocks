﻿#region Using

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
        public Effect CreateShadowSceneEffect()
        {
            var resource = resourceManager.Load("content:Effects/PssmShadowScene");
            return assetManager.Load<Effect>(resource);
        }

        public Effect CreateSssmEffect()
        {
            var resource = resourceManager.Load("content:Effects/Sssm");
            return assetManager.Load<Effect>(resource);
        }

        // I/F
        public ShadowMap CreateShadowMap(ShadowMapSettings shadowMapSettings)
        {
            return new ShadowMap(GraphicsDevice, shadowMapSettings, CreateGaussianBlurEffect());
        }

        // I/F
        public ShadowScene CreateShadowScene(ShadowSettings shadowSettings)
        {
            return new ShadowScene(GraphicsDevice, shadowSettings, CreateShadowSceneEffect());
        }

        // I/F
        public Sssm CreateSssm(SssmSettings sssmSettings)
        {
            return new Sssm(GraphicsDevice, sssmSettings, CreateSssmEffect(), CreateGaussianBlurEffect());
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
