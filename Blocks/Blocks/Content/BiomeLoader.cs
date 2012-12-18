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

        // 非スレッド セーフ
        ComponentFactory componentFactory;

        // 非スレッド セーフ
        ComponentBundleBuilder componentBundleBuilder;

        static BiomeLoader()
        {
            ComponentTypeRegistory = new ComponentTypeRegistory();
            NoiseHelper.SetTypeDefinitionNames(ComponentTypeRegistory);
            // 利用可能な実体の型を全て登録しておく。
            ComponentTypeRegistory.SetTypeDefinitionName(typeof(DefaultBiome));
            ComponentTypeRegistory.SetTypeDefinitionName(typeof(DefaultBiome.Range));
        }

        public BiomeLoader()
        {
            componentFactory = new ComponentFactory(componentInfoManager);
            componentBundleBuilder = new ComponentBundleBuilder(componentInfoManager);
        }

        public object Load(IResource resource)
        {
            var definition = (ComponentBundleDefinition) serializer.Deserialize(resource);

            componentFactory.Build(ref definition);

            var biome = componentFactory[ComponentName] as IBiome;
            
            componentFactory.Clear();

            return biome;
        }

        public void Save(IResource resource, object asset)
        {
            var biome = asset as IBiome;

            componentBundleBuilder.Add(ComponentName, biome);

            ComponentBundleDefinition definition;
            componentBundleBuilder.BuildDefinition(out definition);

            serializer.Serialize(resource, definition);
        }
    }
}
