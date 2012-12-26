#region Using

using System;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Content
{
    public sealed class ChunkProcedureLoader : IAssetLoader
    {
        public const string ComponentName = "Target";

        public static ComponentTypeRegistory ComponentTypeRegistory { get; private set; }

        DefinitionSerializer serializer = new DefinitionSerializer(typeof(ComponentBundleDefinition));

        ComponentInfoManager componentInfoManager = new ComponentInfoManager(ComponentTypeRegistory);

        // 非スレッド セーフ
        ComponentFactory componentFactory;

        // 非スレッド セーフ
        ComponentBundleBuilder componentBundleBuilder;

        static ChunkProcedureLoader()
        {
            ComponentTypeRegistory = new ComponentTypeRegistory();
            // 利用可能な実体の型を全て登録しておく。
            NoiseHelper.SetTypeDefinitionNames(ComponentTypeRegistory);
            ComponentTypeRegistory.SetTypeDefinitionName(typeof(FlatTerrainProcedure));
            ComponentTypeRegistory.SetTypeDefinitionName(typeof(DefaultTerrainProcedure));
        }

        public ChunkProcedureLoader()
        {
            componentFactory = new ComponentFactory(componentInfoManager);
            componentBundleBuilder = new ComponentBundleBuilder(componentInfoManager);
        }

        public object Load(IResource resource)
        {
            var definition = (ComponentBundleDefinition) serializer.Deserialize(resource);

            componentFactory.Build(ref definition);

            var procedure = componentFactory[ComponentName] as IChunkProcedure;

            componentFactory.Clear();

            return procedure;
        }

        public void Save(IResource resource, object asset)
        {
            var procedure = asset as IChunkProcedure;

            componentBundleBuilder.Add(ComponentName, procedure);

            ComponentBundleDefinition definition;
            componentBundleBuilder.BuildDefinition(out definition);

            serializer.Serialize(resource, definition);
        }
    }
}
