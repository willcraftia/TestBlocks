#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Serialization;
using Willcraftia.Xna.Blocks.Assets;
using Willcraftia.Xna.Blocks.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class RegionManager
    {
        static readonly Logger logger = new Logger(typeof(RegionManager).Name);

        //====================================================================
        // Efficiency

        // TODO
        // どこで管理する？
        public VectorI3 ChunkSize = new VectorI3(16, 16, 16);

        //
        //====================================================================

        IServiceProvider serviceProvider;

        GraphicsDevice graphicsDevice;

        List<Region> regions = new List<Region>();

        public RegionManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            var graphicsDeviceService = serviceProvider.GetRequiredService<IGraphicsDeviceService>();
            graphicsDevice = graphicsDeviceService.GraphicsDevice;
        }

        // 非同期呼び出しを想定。
        // Region 丸ごと別スレッドでロードするつもり。
        public Region LoadRegion(Uri uri)
        {
            logger.InfoBegin("LoadRegion: {0}", uri);

            var assetManager = new AssetManager(serviceProvider);
            assetManager.LoaderMap[typeof(Region)] = new RegionLoader();
            assetManager.LoaderMap[typeof(Tile)] = new TileLoader();
            assetManager.LoaderMap[typeof(TileCatalog)] = new TileCatalogLoader(graphicsDevice);
            assetManager.LoaderMap[typeof(Block)] = new BlockLoader();
            assetManager.LoaderMap[typeof(BlockCatalog)] = new BlockCatalogLoader();
            assetManager.LoaderMap[typeof(Mesh)] = new MeshLoader();
            assetManager.LoaderMap[typeof(Texture2D)] = new Texture2DLoader(graphicsDevice);

            var region = assetManager.Load<Region>(uri);
            region.ChunkSize = ChunkSize;
            region.AssetManager = assetManager;
            region.ChunkStore = new StorageChunkStore(uri);
            region.Initialize();

            lock (regions)
            {
                regions.Add(region);
            }

            logger.InfoEnd("LoadRegion: {0}", uri);

            return region;
        }

        public bool RegionExists(ref VectorI3 gridPosition)
        {
            Region region;
            return TryGetRegion(ref gridPosition, out region);
        }

        public bool TryGetRegion(ref VectorI3 gridPosition, out Region region)
        {
            lock (regions)
            {
                foreach (var r in regions)
                {
                    if (r.ContainsGridPosition(ref gridPosition))
                    {
                        region = r;
                        return true;
                    }
                }
            }

            region = null;
            return false;
        }

        public void Update()
        {
            foreach (var region in regions)
                region.Update();
        }
    }
}
