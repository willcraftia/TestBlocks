#region Using

using System;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Component;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Content
{
    public sealed class BiomeManagerLoader : IAssetLoader, IAssetManagerAware
    {
        public const string ComponentName = "Target";

        public static ComponentTypeRegistory ComponentTypeRegistory { get; private set; }

        DefinitionSerializer serializer = new DefinitionSerializer(typeof(ComponentBundleDefinition));

        ComponentInfoManager componentInfoManager = new ComponentInfoManager(ComponentTypeRegistory);

        // 非スレッド セーフ
        ComponentFactory componentFactory;

        // 非スレッド セーフ
        ComponentBundleBuilder componentBundleBuilder;

        // 非スレッド セーフ
        AssetPropertyHandler assetPropertyHandler = new AssetPropertyHandler();

        // I/F
        public AssetManager AssetManager
        {
            set
            {
                assetPropertyHandler.AssetManager = value;
            }
        }

        static BiomeManagerLoader()
        {
            ComponentTypeRegistory = new ComponentTypeRegistory();
            // 利用可能な実体の型を全て登録しておく。
            ComponentTypeRegistory.SetTypeDefinitionName(typeof(SingleBiomeManager));
        }

        public BiomeManagerLoader(ResourceManager resourceManager)
        {
            assetPropertyHandler.ResourceManager = resourceManager;

            componentFactory = new ComponentFactory(componentInfoManager);
            componentFactory.PropertyHandlers.Add(assetPropertyHandler);
            
            componentBundleBuilder = new ComponentBundleBuilder(componentInfoManager);
        }

        public object Load(IResource resource)
        {
            var definition = (ComponentBundleDefinition) serializer.Deserialize(resource);

            assetPropertyHandler.BaseResource = resource;
            componentFactory.Build(ref definition);

            var biomeManager = componentFactory[ComponentName] as IBiomeManager;

            componentFactory.Clear();
            assetPropertyHandler.BaseResource = null;

            biomeManager.Resource = resource;

            return biomeManager;
        }

        public void Save(IResource resource, object asset)
        {
            var biomeManager = asset as IBiomeManager;

            componentBundleBuilder.Add(ComponentName, biomeManager);

            ComponentBundleDefinition definition;
            componentBundleBuilder.BuildDefinition(out definition);

            serializer.Serialize(resource, definition);

            biomeManager.Resource = resource;
        }
    }
}
