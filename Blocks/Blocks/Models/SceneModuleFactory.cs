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

        public SceneModuleFactory(ResourceManager resourceManager, AssetManager assetManager)
        {
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");
            if (assetManager == null) throw new ArgumentNullException("assetManager");

            this.resourceManager = resourceManager;
            this.assetManager = assetManager;
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
            var resource = resourceManager.Load("content:Effects/ShadowScene");
            return assetManager.Load<Effect>(resource);
        }

        // I/F
        public Effect CreateSssmEffect()
        {
            var resource = resourceManager.Load("content:Effects/Sssm");
            return assetManager.Load<Effect>(resource);
        }

        // I/F
        public Effect CreateDepthMapEffect()
        {
            var resource = resourceManager.Load("content:Effects/DepthMap");
            return assetManager.Load<Effect>(resource);
        }

        // I/F
        public Effect CreateDofEffect()
        {
            var resource = resourceManager.Load("content:Effects/Dof");
            return assetManager.Load<Effect>(resource);
        }

        // I/F
        public Effect CreateGaussianBlurEffect()
        {
            var resource = resourceManager.Load("content:Effects/GaussianBlur");
            return assetManager.Load<Effect>(resource);
        }

        // I/F
        public Effect CreateNormalDepthMapEffect()
        {
            var resource = resourceManager.Load("content:Effects/NormalDepthMap");
            return assetManager.Load<Effect>(resource);
        }

        // I/F
        public Effect CreateEdgeEffect()
        {
            var resource = resourceManager.Load("content:Effects/Edge");
            return assetManager.Load<Effect>(resource);
        }
    }
}
