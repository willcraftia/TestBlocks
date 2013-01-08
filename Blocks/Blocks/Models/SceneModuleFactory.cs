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

        public SceneModuleFactory(ResourceManager resourceManager, AssetManager assetManager)
        {
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");
            if (assetManager == null) throw new ArgumentNullException("assetManager");

            this.resourceManager = resourceManager;
            this.assetManager = assetManager;
        }

        // I/F
        public Effect CreateGaussianBlurEffect()
        {
            return LoadAsset<Effect>("content:Effects/GaussianBlur");
        }

        // I/F
        public Effect CreateShadowMapEffect()
        {
            return LoadAsset<Effect>("content:Effects/ShadowMap");
        }

        // I/F
        public Texture2D CreateLensFlareGlowSprite()
        {
            return LoadAsset<Texture2D>("content:Textures/LensFlare/Glow");
        }

        // I/F
        public Texture2D[] CreateLensFlareFlareSprites()
        {
            Texture2D[] sprites =
            {
                LoadAsset<Texture2D>("content:Textures/LensFlare/Flare1"),
                LoadAsset<Texture2D>("content:Textures/LensFlare/Flare2"),
                LoadAsset<Texture2D>("content:Textures/LensFlare/Flare3")
            };
            return sprites;
        }

        T LoadAsset<T>(string uri)
        {
            var resource = resourceManager.Load(uri);
            return assetManager.Load<T>(resource);
        }
    }
}
