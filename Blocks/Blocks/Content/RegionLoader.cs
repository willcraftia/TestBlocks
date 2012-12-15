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

        static readonly ComponentTypeRegistory componentTypeRegistory = new ComponentTypeRegistory();

        ResourceManager resourceManager;

        DefinitionSerializer serializer = new DefinitionSerializer(typeof(RegionDefinition));

        // I/F
        public AssetManager AssetManager { private get; set; }
        
        static RegionLoader()
        {
            componentTypeRegistory.SetTypeDefinitionName(typeof(FlatTerrainProcedureComponent), "FlatTerrain");
        }

        public RegionLoader(ResourceManager resourceManager)
        {
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");

            this.resourceManager = resourceManager;
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

        List<ChunkProcedure> ToChunkProcedures(BundleDefinition[] definitions)
        {
            if (ArrayHelper.IsNullOrEmpty(definitions)) return new List<ChunkProcedure>(0);

            var list = new List<ChunkProcedure>(definitions.Length);
            for (int i = 0; i < definitions.Length; i++)
            {
                var procedure = new ChunkProcedure();

                var factory = new ComponentFactory(componentTypeRegistory);
                factory.Build(ref definitions[i]);

                procedure.Component = factory[ComponentName] as ChunkProcedureComponent;
                procedure.ComponentFactory = factory;

                list.Add(procedure);
            }

            return list;
        }

        BundleDefinition[] ToChunkProcedureDefinition(List<ChunkProcedure> procedures)
        {
            if (CollectionHelper.IsNullOrEmpty(procedures)) return null;

            var definitions = new BundleDefinition[procedures.Count];
            for (int i = 0; i < procedures.Count; i++)
            {
                var procedure = procedures[i];

                var factory = procedure.ComponentFactory;
                if (factory == null)
                {
                    factory = new ComponentFactory(componentTypeRegistory);
                    procedure.ComponentFactory = factory;
                }

                factory.GetDefinition(out definitions[i]);
            }

            return definitions;
        }
    }
}
