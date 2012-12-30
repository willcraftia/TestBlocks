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

        public ShadowMapEffect CreateShadowMapEffect()
        {
            var resource = resourceManager.Load("content:Effects/ShadowMapEffect");
            var effect = assetManager.Load<Effect>(resource);
            return new ShadowMapEffect(effect);
        }

        public Pssm CreatePssm(ShadowSettings shadowSettings)
        {
            return new Pssm(GraphicsDevice, shadowSettings);
        }

        public BasicEffect CreateDebugBoundingBoxEffect()
        {
            return new BasicEffect(GraphicsDevice);
        }

        public BoundingBoxDrawer CreateDebugBoundingBoxDrawer()
        {
            return new BoundingBoxDrawer(GraphicsDevice);
        }
    }
}
