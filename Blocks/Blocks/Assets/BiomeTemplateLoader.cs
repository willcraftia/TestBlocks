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
    public sealed class BiomeTemplateLoader : IAssetLoader
    {
        DefinitionSerializer serializer = new DefinitionSerializer(typeof(BundleDefinition));

        public object Load(IResource resource)
        {
            var definition = (BundleDefinition) serializer.Deserialize(resource);

            var biomeTemplate = new BiomeTemplate
            {
                Resource = resource
            };

            biomeTemplate.ComponentFactory.AddBundleDefinition(ref definition);
            biomeTemplate.ComponentFactory.Build();
            
            return biomeTemplate;
        }

        public void Save(IResource resource, object asset)
        {
            var biomeTemplate = asset as BiomeTemplate;

            BundleDefinition definition;
            biomeTemplate.ComponentFactory.GetDefinition(out definition);

            serializer.Serialize(resource, definition);

            biomeTemplate.Resource = resource;
        }
    }
}
