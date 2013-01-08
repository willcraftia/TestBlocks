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
        public Effect CreateGaussianBlurEffect()
        {
            return LoadAsset<Effect>("content:Effects/GaussianBlur");
        }

        // I/F
        public Effect CreateDepthMapEffect()
        {
            return LoadAsset<Effect>("content:Effects/DepthMap");
        }

        // I/F
        public Effect CreateNormalDepthMapEffect()
        {
            return LoadAsset<Effect>("content:Effects/NormalDepthMap");
        }

        // I/F
        public Effect CreateShadowMapEffect()
        {
            return LoadAsset<Effect>("content:Effects/ShadowMap");
        }

        // I/F
        public Effect CreateShadowSceneEffect()
        {
            return LoadAsset<Effect>("content:Effects/ShadowScene");
        }

        // I/F
        public Effect CreateSssmEffect()
        {
            return LoadAsset<Effect>("content:Effects/Sssm");
        }

        // I/F
        public Effect CreateSsaoMapEffect()
        {
            return LoadAsset<Effect>("content:Effects/SsaoMap");
        }

        // I/F
        public Effect CreateSsaoMapBlurEffect()
        {
            return LoadAsset<Effect>("content:Effects/SsaoMapBlur");
        }

        // I/F
        public Effect CreateSsaoEffect()
        {
            return LoadAsset<Effect>("content:Effects/Ssao");
        }

        // I/F
        public Texture2D CreateRandomNormalMap()
        {
            return LoadAsset<Texture2D>("content:Textures/RandomNormal");
        }

        // I/F
        public Effect CreateEdgeEffect()
        {
            return LoadAsset<Effect>("content:Effects/Edge");
        }

        // I/F
        public Effect CreateBloomExtractEffect()
        {
            return LoadAsset<Effect>("content:Effects/BloomExtract");
        }

        // I/F
        public Effect CreateBloomEffect()
        {
            return LoadAsset<Effect>("content:Effects/Bloom");
        }

        // I/F
        public Effect CreateDofEffect()
        {
            return LoadAsset<Effect>("content:Effects/Dof");
        }

        // I/F
        public Effect CreateMonochromeEffect()
        {
            return LoadAsset<Effect>("content:Effects/Monochrome");
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

        // I/F
        public Effect CreateSnowEffect()
        {
            return LoadAsset<Effect>("content:Effects/Snow");
        }

        // I/F
        public Texture2D CreateSnowSprite()
        {
            return LoadAsset<Texture2D>("content:Textures/Snow/Snow");
        }

        // I/F
        public Effect CreateParticleEffect()
        {
            return LoadAsset<Effect>("content:Effects/Particle");
        }

        T LoadAsset<T>(string uri)
        {
            var resource = resourceManager.Load(uri);
            return assetManager.Load<T>(resource);
        }
    }
}
