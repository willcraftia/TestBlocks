#region Using

using System;
using System.Collections.Generic;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Assets;
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

        // I/F
        public AssetManager AssetManager { private get; set; }

        // I/F
        public ResourceManager ResourceManager { private get; set; }

        // I/F
        public object Load(IResource resource)
        {
            var definition = (RegionDefinition) serializer.Deserialize(resource);

            var region = new Region();

            region.Resource = resource;
            region.Name = definition.Name;
            region.Bounds = definition.Bounds;

            if (!string.IsNullOrEmpty(definition.TileCatalog))
            {
                var tileCatalogResource = ResourceManager.Load(resource, definition.TileCatalog);
                region.TileCatalog = AssetManager.Load<TileCatalog>(tileCatalogResource);
            }

            if (!string.IsNullOrEmpty(definition.BlockCatalog))
            {
                var blockCatalogResource = ResourceManager.Load(resource, definition.BlockCatalog);
                region.BlockCatalog = AssetManager.Load<BlockCatalog>(blockCatalogResource);
            }

            if (!string.IsNullOrEmpty(definition.ChunkBundle))
            {
                var chunkBundleResource = ResourceManager.Load(resource, definition.ChunkBundle);
                region.ChunkBundleResource = chunkBundleResource;
            }

            if (ArrayHelper.IsNullOrEmpty(definition.ChunkProcedures))
            {
                region.ChunkProcesures = new List<IProcedure<Chunk>>(0);
            }
            else
            {
                region.ChunkProcesures = new List<IProcedure<Chunk>>(definition.ChunkProcedures.Length);
                for (int i = 0; i < definition.ChunkProcedures.Length; i++)
                {
                    var procedure = Procedures.ToProcedure<Chunk>(ref definition.ChunkProcedures[i]);
                    region.ChunkProcesures.Add(procedure);
                }
            }

            return region;
        }

        // I/F
        public void Save(IResource resource, object asset)
        {
            var region = asset as Region;

            var definition = new RegionDefinition();

            definition.Name = region.Name;
            definition.Bounds = region.Bounds;
            
            if (region.TileCatalog != null && region.TileCatalog.Resource != null)
                definition.TileCatalog = ResourceManager.CreateRelativeUri(resource, region.TileCatalog.Resource);
            
            if (region.BlockCatalog != null && region.BlockCatalog.Resource != null)
                definition.BlockCatalog = ResourceManager.CreateRelativeUri(resource, region.BlockCatalog.Resource);

            if (region.ChunkBundleResource != null)
                definition.ChunkBundle = ResourceManager.CreateRelativeUri(resource, region.ChunkBundleResource);

            if (!CollectionHelper.IsNullOrEmpty(definition.ChunkProcedures))
            {
                definition.ChunkProcedures = new ProcedureDefinition[region.ChunkProcesures.Count];
                for (int i = 0; i < region.ChunkProcesures.Count; i++)
                {
                    definition.ChunkProcedures[i] = new ProcedureDefinition();
                    Procedures.ToDefinition(region.ChunkProcesures[i], out definition.ChunkProcedures[i]);
                }
            }

            serializer.Serialize(resource, definition);
        }
    }
}
