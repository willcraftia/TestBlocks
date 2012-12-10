#region Using

using System;
using System.Collections.Generic;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Blocks.Models;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public sealed class RegionLoader : AssetLoaderBase
    {
        static readonly Logger logger = new Logger(typeof(RegionLoader).Name);

        DefinitionSerializer serializer = new DefinitionSerializer(typeof(RegionDefinition));

        public RegionLoader(UriManager uriManager)
            : base(uriManager)
        {
        }

        public override object Load(IUri uri)
        {
            var resource = (RegionDefinition) serializer.Deserialize(uri);

            var region = new Region();

            region.Uri = uri;
            region.Name = resource.Name;
            region.Bounds = resource.Bounds;

            if (!string.IsNullOrEmpty(resource.TileCatalog))
            {
                var tileCatalogUri = UriManager.Create(uri.BaseUri, resource.TileCatalog);
                region.TileCatalog = AssetManager.Load<TileCatalog>(tileCatalogUri);
            }

            if (!string.IsNullOrEmpty(resource.BlockCatalog))
            {
                var blockCatalogUri = UriManager.Create(uri.BaseUri, resource.BlockCatalog);
                region.BlockCatalog = AssetManager.Load<BlockCatalog>(blockCatalogUri);
            }

            if (!string.IsNullOrEmpty(resource.ChunkBundle))
            {
                var chunkBundleUri = UriManager.Create(uri.BaseUri, resource.ChunkBundle);
                region.ChunkBundleUri = chunkBundleUri;
            }

            if (ArrayHelper.IsNullOrEmpty(resource.ChunkProcedures))
            {
                region.ChunkProcesures = new List<IProcedure<Chunk>>(0);
            }
            else
            {
                region.ChunkProcesures = new List<IProcedure<Chunk>>(resource.ChunkProcedures.Length);
                for (int i = 0; i < resource.ChunkProcedures.Length; i++)
                {
                    var procedure = Procedures.ToProcedure<Chunk>(ref resource.ChunkProcedures[i]);
                    region.ChunkProcesures.Add(procedure);
                }
            }

            return region;
        }

        public override void Save(IUri uri, object asset)
        {
            var region = asset as Region;

            var resource = new RegionDefinition();

            resource.Name = region.Name;
            resource.Bounds = region.Bounds;
            
            if (region.TileCatalog != null && region.TileCatalog.Uri != null)
                resource.TileCatalog = UriManager.CreateRelativeUri(uri.BaseUri, region.TileCatalog.Uri);
            
            if (region.BlockCatalog != null && region.BlockCatalog.Uri != null)
                resource.BlockCatalog = UriManager.CreateRelativeUri(uri.BaseUri, region.BlockCatalog.Uri);

            if (region.ChunkBundleUri != null)
                resource.ChunkBundle = UriManager.CreateRelativeUri(uri.BaseUri, region.ChunkBundleUri);

            if (!CollectionHelper.IsNullOrEmpty(resource.ChunkProcedures))
            {
                resource.ChunkProcedures = new ProcedureDefinition[region.ChunkProcesures.Count];
                for (int i = 0; i < region.ChunkProcesures.Count; i++)
                {
                    resource.ChunkProcedures[i] = new ProcedureDefinition();
                    Procedures.ToDefinition(region.ChunkProcesures[i], out resource.ChunkProcedures[i]);
                }
            }

            serializer.Serialize(uri, resource);
        }
    }
}
