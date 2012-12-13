#region Using

using System;
using System.Collections.Generic;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class RegionLoader : IAssetLoader, IAssetManagerAware, IResourceManagerAware
    {
        static readonly Logger logger = new Logger(typeof(RegionLoader).Name);

        DefinitionSerializer serializer = new DefinitionSerializer(typeof(RegionDefinition));

        // for procedures and noise sources.
        AliasTypeRegistory componentTypeRegistory = new AliasTypeRegistory();

        // I/F
        public AssetManager AssetManager { private get; set; }

        // I/F
        public ResourceManager ResourceManager { private get; set; }

        public RegionLoader()
        {
            // todo: ComponentTypeRegistory
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
                ChunkBundle = ToUri(resource, region.ChunkBundleResource),
                ChunkProcedures = ToProcedureDefinitions(region.ChunkProcesures)
            };

            serializer.Serialize(resource, definition);

            region.Resource = resource;
        }

        IResource Load(IResource baseResource, string uri)
        {
            if (string.IsNullOrEmpty(uri)) return null;

            return ResourceManager.Load(baseResource, uri);
        }

        T Load<T>(IResource baseResource, string uri) where T : class
        {
            if (string.IsNullOrEmpty(uri)) return null;

            var resource = ResourceManager.Load(baseResource, uri);
            return AssetManager.Load<T>(resource);
        }

        List<IProcedure<Chunk>> ToChunkProcedures(ProcedureDefinition[] definitions)
        {
            if (ArrayHelper.IsNullOrEmpty(definitions)) return new List<IProcedure<Chunk>>(0);

            var list = new List<IProcedure<Chunk>>(definitions.Length);
            for (int i = 0; i < definitions.Length; i++)
            {
                var procedure = ToChunkProcedure(ref definitions[i]);
                list.Add(procedure);
            }

            return list;
        }

        IProcedure<Chunk> ToChunkProcedure(ref ProcedureDefinition definition)
        {
            var factory = new ComponentBundleFactory(componentTypeRegistory);
            factory.Initialize(ref definition.ComponentBundle);

            return factory[definition.Target] as IProcedure<Chunk>;
        }

        string ToUri(IResource baseResource, IResource resource)
        {
            if (resource == null) return null;

            return ResourceManager.CreateRelativeUri(baseResource, resource);
        }

        string ToUri(IResource baseResource, IAsset asset)
        {
            if (asset == null || asset.Resource == null) return null;

            return ResourceManager.CreateRelativeUri(baseResource, asset.Resource);
        }

        ProcedureDefinition[] ToProcedureDefinitions(List<IProcedure<Chunk>> procedures)
        {
            if (CollectionHelper.IsNullOrEmpty(procedures)) return null;

            var definitions = new ProcedureDefinition[procedures.Count];
            for (int i = 0; i < procedures.Count; i++)
                ToProcedureDefinition(procedures[i], out definitions[i]);

            return definitions;
        }

        void ToProcedureDefinition(IProcedure<Chunk> procedure, out ProcedureDefinition definition)
        {
            definition = new ProcedureDefinition { Target = procedure.ComponentName };

            var factory = procedure.ComponentBundleFactory;
            if (factory == null)
            {
                factory = new ComponentBundleFactory(componentTypeRegistory);
                procedure.ComponentBundleFactory = factory;
            }

            factory.GetDefinition(out definition.ComponentBundle);
        }
    }
}
