#region Using

using System;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Noise;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Content
{
    public sealed class BiomeLoader : IAssetLoader
    {
        public const string ComponentName = "Target";

        public static ComponentTypeRegistory ComponentTypeRegistory { get; private set; }

        DefinitionSerializer serializer = new DefinitionSerializer(typeof(ComponentBundleDefinition));

        static BiomeLoader()
        {
            ComponentTypeRegistory = new ComponentTypeRegistory();

            NoiseHelper.SetTypeDefinitionNames(ComponentTypeRegistory);

            // 利用可能な実体の型を全て登録しておく。
            ComponentTypeRegistory.SetTypeDefinitionName(typeof(DefaultBiomeCore));
            ComponentTypeRegistory.SetTypeDefinitionName(typeof(DefaultBiomeCore.Range));
        }

        public object Load(IResource resource)
        {
            var definition = (ComponentBundleDefinition) serializer.Deserialize(resource);

            var biome = new Biome { Resource = resource };

            var factory = new ComponentFactory(ComponentTypeRegistory);
            factory.Build(ref definition);
            biome.Core = factory[ComponentName] as IBiomeCore;
            biome.ComponentFactory = factory;
            
            return biome;
        }

        public void Save(IResource resource, object asset)
        {
            var biome = asset as Biome;

            //var factory = biome.ComponentFactory;
            //if (factory == null)
            //{
            //    factory = new ComponentFactory(componentTypeRegistory);
            //    biome.ComponentFactory = factory;
            //}

            //BundleDefinition definition;
            //factory.GetDefinition(out definition);

            //serializer.Serialize(resource, definition);

            biome.Resource = resource;
        }
    }
}
