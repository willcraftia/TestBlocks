#region Using

using System;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Noise;
using Willcraftia.Xna.Blocks.Component;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Content
{
    public sealed class NoiseLoader : IAssetLoader, IAssetManagerAware
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

        static NoiseLoader()
        {
            ComponentTypeRegistory = new ComponentTypeRegistory();
            NoiseHelper.SetTypeDefinitionNames(ComponentTypeRegistory);
        }

        public NoiseLoader(ResourceManager resourceManager)
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

            var noiseSource = componentFactory[ComponentName] as INoiseSource;
            
            componentFactory.Clear();
            assetPropertyHandler.BaseResource = null;

            return noiseSource;
        }

        public void Save(IResource resource, object asset)
        {
            var noiseSource = asset as INoiseSource;

            // TODO
            //
            // アセット参照のプロパティをどうするのか？

            componentBundleBuilder.Add(ComponentName, noiseSource);

            ComponentBundleDefinition definition;
            componentBundleBuilder.BuildDefinition(out definition);

            serializer.Serialize(resource, definition);
        }
    }
}
