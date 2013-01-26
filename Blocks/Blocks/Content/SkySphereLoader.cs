#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Content
{
    public sealed class SkySphereLoader : IAssetLoader, IAssetManagerAware
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(SkySphereDefinition));

        ResourceManager resourceManager;

        GraphicsDevice graphicsDevice;

        // I/F
        public AssetManager AssetManager { private get; set; }

        public SkySphereLoader(ResourceManager resourceManager, GraphicsDevice graphicsDevice)
        {
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            this.resourceManager = resourceManager;
            this.graphicsDevice = graphicsDevice;
        }

        public object Load(IResource resource)
        {
            var definition = (SkySphereDefinition) serializer.Deserialize(resource);

            // TODO
            // オブジェクト名も設定で管理か？
            return new SkySphere("skySphere", graphicsDevice)
            {
                SunVisible = definition.SunVisible,
                SunThreshold = definition.SunThreshold,
                Effect = new SkySphereEffect(Load<Effect>("content:Effects/SkySphere"))
            };
        }

        public void Save(IResource resource, object asset)
        {
            var skySphere = asset as SkySphere;

            var definition = new SkySphereDefinition
            {
                SunVisible = skySphere.SunVisible,
                SunThreshold = skySphere.SunThreshold
            };

            serializer.Serialize(resource, definition);
        }

        T Load<T>(string uri) where T : class
        {
            if (string.IsNullOrEmpty(uri)) return null;

            var resource = resourceManager.Load(uri);
            return AssetManager.Load<T>(resource);
        }
    }
}
