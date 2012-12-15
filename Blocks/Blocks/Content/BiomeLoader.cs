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

        ComponentInfoManager componentInfoManager = new ComponentInfoManager(ComponentTypeRegistory);

        // スレッド セーフではない使い方をします。
        ComponentFactory componentFactory;

        static BiomeLoader()
        {
            ComponentTypeRegistory = new ComponentTypeRegistory();
            NoiseHelper.SetTypeDefinitionNames(ComponentTypeRegistory);
            // 利用可能な実体の型を全て登録しておく。
            ComponentTypeRegistory.SetTypeDefinitionName(typeof(DefaultBiomeCore));
            ComponentTypeRegistory.SetTypeDefinitionName(typeof(DefaultBiomeCore.Range));
        }

        public BiomeLoader()
        {
            componentFactory = new ComponentFactory(componentInfoManager);
        }

        public object Load(IResource resource)
        {
            var definition = (ComponentBundleDefinition) serializer.Deserialize(resource);

            var biome = new Biome { Resource = resource };

            componentFactory.Build(ref definition);
            biome.Core = componentFactory[ComponentName] as IBiomeCore;
            componentFactory.Clear();

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
