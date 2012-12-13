﻿#region Using

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
        AliasTypeRegistory typeRegistory = new AliasTypeRegistory();

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
                ChunkProcedures = ToComponentDefinitions(region.ChunkProcesures)
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

        List<IProcedure<Chunk>> ToChunkProcedures(ComponentDefinition[] definitions)
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

        IProcedure<Chunk> ToChunkProcedure(ref ComponentDefinition definition)
        {
            var factory = new ComponentFactory(typeRegistory);
            var component = factory.CreateComponent(ref definition);
            return component as IProcedure<Chunk>;
        }

        ComponentDefinition[] ToComponentDefinitions(List<IProcedure<Chunk>> procedures)
        {
            if (CollectionHelper.IsNullOrEmpty(procedures)) return null;

            var definitions = new ComponentDefinition[procedures.Count];
            for (int i = 0; i < procedures.Count; i++)
                ToComponentDefinition(procedures[i], out definitions[i]);

            return definitions;
        }

        void ToComponentDefinition(IProcedure<Chunk> procedure, out ComponentDefinition definition)
        {
            var factory = procedure.ComponentFactory;
            if (factory == null)
            {
                factory = new ComponentFactory(typeRegistory);
                procedure.ComponentFactory = factory;
            }
            factory.CreateDefinition(procedure, out definition);
        }
    }
}
