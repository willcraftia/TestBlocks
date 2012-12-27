#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Content
{
    public sealed class SkySphereLoader : IAssetLoader, IAssetManagerAware
    {
        GraphicsDevice graphicsDevice;

        ResourceManager resourceManager;

        DefinitionSerializer serializer = new DefinitionSerializer(typeof(SkySphereDefinition));

        // I/F
        public AssetManager AssetManager { private get; set; }

        public SkySphereLoader(GraphicsDevice graphicsDevice, ResourceManager resourceManager)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");

            this.graphicsDevice = graphicsDevice;
            this.resourceManager = resourceManager;
        }

        public object Load(IResource resource)
        {
            var definition = (SkySphereDefinition) serializer.Deserialize(resource);

            var skySphere = new SkySphere(graphicsDevice);

            if (!ArrayHelper.IsNullOrEmpty(definition.ColorTable))
            {
                for (int i = 0; i < definition.ColorTable.Length; i++)
                {
                    var skyColor = ToSkyColor(ref definition.ColorTable[i]);
                    skySphere.ColorTable.AddColor(skyColor);
                }
            }

            var effectResource = resourceManager.Load("content:Effects/SkySphereEffect");
            skySphere.Effect = new SkySphereEffect(AssetManager.Load<Effect>(effectResource));

            return skySphere;
        }

        SkyColor ToSkyColor(ref SkyColorDefinition definition)
        {
            return new SkyColor(definition.Time, definition.Color);
        }

        public void Save(IResource resource, object asset)
        {
            var skySphere = asset as SkySphere;

            var definition = new SkySphereDefinition();

            if (skySphere.ColorTable.Count != 0)
            {
                definition.ColorTable = new SkyColorDefinition[skySphere.ColorTable.Count];
                int index = 0;
                foreach (var skyColor in skySphere.ColorTable)
                {
                    definition.ColorTable[index++] = new SkyColorDefinition
                    {
                        Time = skyColor.Time,
                        Color = skyColor.Color
                    };
                }
            }

            serializer.Serialize(resource, definition);
        }

        T Load<T>(IResource baseResource, string uri) where T : class
        {
            if (string.IsNullOrEmpty(uri)) return null;

            var resource = resourceManager.Load(baseResource, uri);
            return AssetManager.Load<T>(resource);
        }

        string ToUri(IResource baseResource, IResource resource)
        {
            if (resource == null) return null;

            return resourceManager.CreateRelativeUri(baseResource, resource);
        }

        string ToUri(IResource baseResource, IAsset asset)
        {
            if (asset == null || asset.Resource == null) return null;

            return resourceManager.CreateRelativeUri(baseResource, asset.Resource);
        }
    }
}
