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
        public const string ComponentName = "Target";

        static readonly ComponentTypeRegistory componentTypeRegistory = new ComponentTypeRegistory();

        DefinitionSerializer serializer = new DefinitionSerializer(typeof(BundleDefinition));

        static BiomeLoader()
        {
            NoiseHelper.SetTypeDefinitionNames(componentTypeRegistory);

            // 利用可能な実体の型を全て登録しておく。
            componentTypeRegistory.SetTypeDefinitionName(typeof(BiomeComponent));
            componentTypeRegistory.SetTypeDefinitionName(typeof(BiomeComponent.Range));
        }

        public object Load(IResource resource)
        {
            var definition = (BundleDefinition) serializer.Deserialize(resource);

            var biome = new Biome { Resource = resource };

            var factory = new ComponentFactory(componentTypeRegistory);
            factory.Build(ref definition);

            biome.Component = factory[ComponentName] as IBiomeComponent;
            biome.ComponentFactory = factory;
            
            return biome;
        }

        public void Save(IResource resource, object asset)
        {
            var biome = asset as Biome;

            var factory = biome.ComponentFactory;
            if (factory == null)
            {
                factory = new ComponentFactory(componentTypeRegistory);
                biome.ComponentFactory = factory;
            }

            BundleDefinition definition;
            factory.GetDefinition(out definition);

            serializer.Serialize(resource, definition);

            biome.Resource = resource;
        }
    }
}
