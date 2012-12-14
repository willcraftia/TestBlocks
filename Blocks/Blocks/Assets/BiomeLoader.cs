#region Using

using System;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Noise;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class BiomeLoader : IAssetLoader
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(BundleDefinition));

        public object Load(IResource resource)
        {
            var definition = (BundleDefinition) serializer.Deserialize(resource);

            var biome = new Biome { Resource = resource };

            biome.ComponentFactory.AddBundleDefinition(ref definition);
            biome.ComponentFactory.Build();
            
            return biome;
        }

        public void Save(IResource resource, object asset)
        {
            var biome = asset as Biome;

            BundleDefinition definition;
            biome.ComponentFactory.GetDefinition(out definition);

            serializer.Serialize(resource, definition);

            biome.Resource = resource;
        }
    }
}
