#region Using

using System;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Content
{
    public sealed class ParticleSettingsLoader : IAssetLoader, IAssetManagerAware
    {
        ResourceManager resourceManager;

        DefinitionSerializer serializer = new DefinitionSerializer(typeof(ParticleSettingsDefinition));

        // I/F
        public AssetManager AssetManager { private get; set; }

        public ParticleSettingsLoader(ResourceManager resourceManager)
        {
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");

            this.resourceManager = resourceManager;
        }

        public object Load(IResource resource)
        {
            var definition = (ParticleSettingsDefinition) serializer.Deserialize(resource);

            var particleSettings = new ParticleSettings
            {
                Name = definition.Name,
                MaxParticles = definition.MaxParticles,
                Duration = TimeSpan.FromSeconds(definition.Duration),
                DurationRandomness = definition.DurationRandomness,
                EmitterVelocitySensitivity = definition.EmitterVelocitySensitivity,
                MinHorizontalVelocity = definition.MinHorizontalVelocity,
                MaxHorizontalVelocity = definition.MaxHorizontalVelocity,
                MinVerticalVelocity = definition.MinVerticalVelocity,
                MaxVerticalVelocity = definition.MaxVerticalVelocity,
                Gravity = definition.Gravity,
                EndVelocity = definition.EndVelocity,
                MinColor = new Color(definition.MinColor),
                MaxColor = new Color(definition.MaxColor),
                MinRotateSpeed = definition.MinRotateSpeed,
                MaxRotateSpeed = definition.MaxRotateSpeed,
                MinStartSize = definition.MinStartSize,
                MaxStartSize = definition.MaxStartSize,
                MinEndSize = definition.MinEndSize,
                MaxEndSize = definition.MaxEndSize,
                BlendState = definition.BlendState
            };

            // content 以外もサポートするために Image2D でテクスチャをロード。
            var image = Load<Image2D>(resource, definition.Texture);
            // リソース タグに Image2D をバインド。
            image.Texture.Tag = image;

            particleSettings.Texture = image.Texture;


            return particleSettings;
        }

        public void Save(IResource resource, object asset)
        {
            var particleSettings = asset as ParticleSettings;
            if (particleSettings.Texture == null) throw new InvalidOperationException("Texture is null.");

            //
            // テクスチャのリソース タグに Image2D がバインドされていることを前提とする。
            //

            var image = particleSettings.Texture.Tag as Image2D;
            if (image == null || image.Texture != particleSettings.Texture)
                throw new InvalidOperationException("Invalid texture state.");

            var definition = new ParticleSettingsDefinition
            {
                Name = particleSettings.Name,
                MaxParticles = particleSettings.MaxParticles,
                Duration = particleSettings.Duration.TotalSeconds,
                DurationRandomness = particleSettings.DurationRandomness,
                EmitterVelocitySensitivity = particleSettings.EmitterVelocitySensitivity,
                MinHorizontalVelocity = particleSettings.MinHorizontalVelocity,
                MaxHorizontalVelocity = particleSettings.MaxHorizontalVelocity,
                MinVerticalVelocity = particleSettings.MinVerticalVelocity,
                MaxVerticalVelocity = particleSettings.MaxVerticalVelocity,
                Gravity = particleSettings.Gravity,
                EndVelocity = particleSettings.EndVelocity,
                MinColor = particleSettings.MinColor.ToVector4(),
                MaxColor = particleSettings.MaxColor.ToVector4(),
                MinRotateSpeed = particleSettings.MinRotateSpeed,
                MaxRotateSpeed = particleSettings.MaxRotateSpeed,
                MinStartSize = particleSettings.MinStartSize,
                MaxStartSize = particleSettings.MaxStartSize,
                MinEndSize = particleSettings.MinEndSize,
                MaxEndSize = particleSettings.MaxEndSize,
                Texture = ToUri(resource, image),
                BlendState = particleSettings.BlendState
            };
        }

        T Load<T>(IResource baseResource, string uri) where T : class
        {
            if (string.IsNullOrEmpty(uri)) return null;

            var resource = resourceManager.Load(baseResource, uri);
            return AssetManager.Load<T>(resource);
        }

        string ToUri(IResource baseResource, IAsset asset)
        {
            if (asset == null || asset.Resource == null) return null;

            return resourceManager.CreateRelativeUri(baseResource, asset.Resource);
        }
    }
}
