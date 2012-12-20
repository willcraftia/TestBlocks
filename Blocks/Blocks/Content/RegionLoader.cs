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

        // 非スレッド セーフ
        ComponentFactory componentFactory;

        // 非スレッド セーフ
        ComponentBundleBuilder componentBundleBuilder;

        // I/F
        public AssetManager AssetManager { private get; set; }

        static RegionLoader()
        {
            ComponentTypeRegistory = new ComponentTypeRegistory();
            ComponentTypeRegistory.SetTypeDefinitionName(typeof(FlatTerrainProcedure), "FlatTerrain");
        }

        public RegionLoader(ResourceManager resourceManager)
        {
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");

            this.resourceManager = resourceManager;

            componentFactory = new ComponentFactory(componentInfoManager);
            componentBundleBuilder = new ComponentBundleBuilder(componentInfoManager);
        }

        // I/F
        public object Load(IResource resource)
        {
            var definition = (RegionDefinition) serializer.Deserialize(resource);

            //
            // リージョンのロードでは、必ず、ブロック カタログよりもタイル カタログを先にロードする。
            // これは、使用するタイルに合わせたブロック メッシュでのテクスチャ座標の調整に関係する。
            //

            var region = new Region
            {
                Name = definition.Name,
                Bounds = definition.Bounds,
                TileCatalog = Load<TileCatalog>(resource, definition.TileCatalog),
                BlockCatalog = Load<BlockCatalog>(resource, definition.BlockCatalog),
                BiomeManager = Load<IBiomeManager>(resource, definition.BiomeManager),
                ChunkBundleResource = Load(resource, definition.ChunkBundle)
            };
            region.ChunkProcesures = ToChunkProcedures(resource, definition.ChunkProcedures, region);

            return region;
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
                BiomeManager = ToUri(resource, region.BiomeManager),
                ChunkBundle = ToUri(resource, region.ChunkBundleResource),
                ChunkProcedures = ToUri(resource, region.ChunkProcesures)
            };

            serializer.Serialize(resource, definition);
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

        List<IChunkProcedure> ToChunkProcedures(IResource baseResource, string[] chunkProcedureUris, Region region)
        {
            if (ArrayHelper.IsNullOrEmpty(chunkProcedureUris)) return new List<IChunkProcedure>(0);

            var list = new List<IChunkProcedure>(chunkProcedureUris.Length);
            for (int i = 0; i < chunkProcedureUris.Length; i++)
            {
                var procedure = Load<IChunkProcedure>(baseResource, chunkProcedureUris[i]);
                
                // TODO: コンテキストを設定し、コンテキスト経由で Region を得るように変更。
                procedure.Region = region;

                list.Add(procedure);
            }

            return list;
        }

        string[] ToUri(IResource baseResource, List<IChunkProcedure> procedures)
        {
            if (CollectionHelper.IsNullOrEmpty(procedures)) return null;

            var uris = new string[procedures.Count];
            for (int i = 0; i < procedures.Count; i++)
                uris[i] = ToUri(baseResource, procedures[i].Resource);

            return uris;
        }
    }
}
