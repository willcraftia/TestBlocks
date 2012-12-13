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
        public const string BiomeTemplateComponentName = "BiomeTemplate";

        DefinitionSerializer serializer = new DefinitionSerializer(typeof(BundleDefinition));

        AliasTypeRegistory typeRegistory = new AliasTypeRegistory();

        public BiomeTemplateLoader()
        {
            typeRegistory.SetTypeAlias(typeof(Perlin));
            typeRegistory.SetTypeAlias(typeof(SumFractal));
            typeRegistory.SetTypeAlias(typeof(BiomeTemplate));
        }

        public object Load(IResource resource)
        {
            var definition = (BundleDefinition) serializer.Deserialize(resource);

            var factory = new ComponentBundleFactory(typeRegistory);
            factory.Initialize(ref definition);
            factory.Build();

            var biomeTemplate = factory[BiomeTemplateComponentName] as BiomeTemplate;
            biomeTemplate.Resource = resource;
            return biomeTemplate;
        }

        public void Save(IResource resource, object asset)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
