#region Using

using System;
using System.Collections.Generic;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Content
{
    public sealed class RegionLoader : IAssetLoader, IAssetManagerAware
    {
        public const string ComponentName = "Target";

        static readonly Logger logger = new Logger(typeof(RegionLoader).Name);

        public static ComponentTypeRegistory ComponentTypeRegistory { get; private set; }

        ResourceManager resourceManager;

        DefinitionSerializer serializer = new DefinitionSerializer(typeof(RegionDefinition));

        ComponentInfoManager componentInfoManager = new ComponentInfoManager(ComponentTypeRegistory);

        // スレッド セーフではない使い方をします。
        ComponentFactory componentFactory;

        // I/F
        public AssetManager AssetManager { private get; set; }

        static RegionLoader()
        {
            ComponentTypeRegistory = new ComponentTypeRegistory();
            ComponentTypeRegistory.SetTypeDefinitionName(typeof(FlatTerrainProcedureComponent), "FlatTerrain");
        }

        public RegionLoader(ResourceManager resourceManager)
        {
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");

            this.resourceManager = resourceManager;

            componentFactory = new ComponentFactory(componentInfoManager);
        }

        // I/F
        public object Load(IResource resource)
        {
            var definition = (RegionDefinition) serializer.Deserialize(resource);

            return new Region
            {
                Resource = resource,
                Name = definition.Name,
                Bounds = definition.Bounds,
                TileCatalog = Load<TileCatalog>(resource, definition.TileCatalog),
                BlockCatalog = Load<BlockCatalog>(resource, definition.BlockCatalog),
                BiomeCatalog = Load<BiomeCatalog>(resource, definition.BiomeCatalog),
                ChunkBundleResource = Load(resource, definition.ChunkBundle),
                ChunkProcesures = ToChunkProcedures(definition.ChunkProcedures)
            };
        }

        // I/F
        public void Save(IResource resource, object asset)
        {
            var region = asset as Region;

            var definition = new RegionDefinition
            {
                Name = region.Name,
                Bounds = region.Bounds,
                TileCatalog = ToUri(resource, region.TileCatalog),
                BlockCatalog = ToUri(resource, region.BlockCatalog),
                BiomeCatalog = ToUri(resource, region.BiomeCatalog),
                ChunkBundle = ToUri(resource, region.ChunkBundleResource),
                ChunkProcedures = ToChunkProcedureDefinition(region.ChunkProcesures)
            };

            serializer.Serialize(resource, definition);

            region.Resource = resource;
        }

        IResource Load(IResource baseResource, string uri)
        {
            if (string.IsNullOrEmpty(uri)) return null;

            return resourceManager.Load(baseResource, uri);
        }

        T Load<T>(IResource baseResource, string uri) where T : class
        {
            if (string.IsNullOrEmpty(uri)) return null;

            var resource = resourceManager.Load(baseResource, uri);
            return AssetManager.Load<T>(resource);
        }

        string ToUri(IResource baseResource, IResource resource)
        {
            if (resource == null) return null;

            return resourceManager.CreateRelativeUri(baseResource, resource);
        }

        string ToUri(IResource baseResource, IAsset asset)
        {
            if (asset == null || asset.Resource == null) return null;

            return resourceManager.CreateRelativeUri(baseResource, asset.Resource);
        }

        List<ChunkProcedure> ToChunkProcedures(ComponentBundleDefinition[] definitions)
        {
            if (ArrayHelper.IsNullOrEmpty(definitions)) return new List<ChunkProcedure>(0);

            var list = new List<ChunkProcedure>(definitions.Length);
            for (int i = 0; i < definitions.Length; i++)
            {
                var procedure = new ChunkProcedure();

                componentFactory.Build(ref definitions[i]);
                procedure.Component = componentFactory[ComponentName] as ChunkProcedureComponent;
                componentFactory.Clear();

                list.Add(procedure);
            }

            return list;
        }

        ComponentBundleDefinition[] ToChunkProcedureDefinition(List<ChunkProcedure> procedures)
        {
            if (CollectionHelper.IsNullOrEmpty(procedures)) return null;

            var definitions = new ComponentBundleDefinition[procedures.Count];
            for (int i = 0; i < procedures.Count; i++)
            {
                var procedure = procedures[i];

                //var factory = procedure.ComponentFactory;
                //if (factory == null)
                //{
                //    factory = new ComponentFactory(ComponentTypeRegistory);
                //    procedure.ComponentFactory = factory;
                //}

                //factory.GetDefinition(out definitions[i]);
            }

            return definitions;
        }
    }
}
