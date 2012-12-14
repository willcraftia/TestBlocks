#region Using

using System;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class BiomeCatalogLoader : IAssetLoader, IAssetManagerAware, IResourceManagerAware
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(BiomeCatalogDefinition));

        // I/F
        public AssetManager AssetManager { private get; set; }

        // I/F
        public ResourceManager ResourceManager { private get; set; }

        public object Load(IResource resource)
        {
            var definition = (BiomeCatalogDefinition) serializer.Deserialize(resource);

            var biomeCatalog = new BiomeCatalog(definition.Entries.Length)
            {
                Resource = resource,
                Name = definition.Name
            };

            foreach (var entry in definition.Entries)
            {
                var biome = Load<Biome>(resource, entry.Uri);
                if (biome != null)
                {
                    biome.Index = entry.Index;
                    biomeCatalog.Add(biome);
                }
            }

            return biomeCatalog;
        }

        public void Save(IResource resource, object asset)
        {
            var biomeCatalog = asset as BiomeCatalog;

            var definition = new BiomeCatalogDefinition
            {
                Name = biomeCatalog.Name,
                Entries = new IndexedUriDefinition[biomeCatalog.Count]
            };

            for (int i = 0; i < biomeCatalog.Count; i++)
            {
                var biome = biomeCatalog[i];
                definition.Entries[i] = new IndexedUriDefinition
                {
                    Index = biome.Index,
                    Uri = ToUri(resource, biome)
                };
            }

            serializer.Serialize(resource, definition);

            biomeCatalog.Resource = resource;
        }

        T Load<T>(IResource baseResource, string uri) where T : class
        {
            if (string.IsNullOrEmpty(uri)) return null;

            var resource = ResourceManager.Load(baseResource, uri);
            return AssetManager.Load<T>(resource);
        }

        string ToUri(IResource baseResource, IAsset asset)
        {
            if (asset == null || asset.Resource == null) return null;

            return ResourceManager.CreateRelativeUri(baseResource, asset.Resource);
        }
    }
}
