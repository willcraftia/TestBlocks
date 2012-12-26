#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Noise;
using Willcraftia.Xna.Framework.Serialization;
using Willcraftia.Xna.Blocks.Content;
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
        public static VectorI3 ChunkSize = new VectorI3(16, 16, 16);

        //
        //====================================================================

        IServiceProvider serviceProvider;

        GraphicsDevice graphicsDevice;

        List<Region> regions = new List<Region>();

        public RegionManager(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");

            this.serviceProvider = serviceProvider;

            var graphicsDeviceService = serviceProvider.GetRequiredService<IGraphicsDeviceService>();
            graphicsDevice = graphicsDeviceService.GraphicsDevice;
        }

        //
        // AssetManager の扱い
        //
        // エディタ:
        //      エディタで一つの AssetManager。
        //      各モデルは、必ずしも Region に関連付けられるとは限らない。
        //
        // ゲーム環境:
        //      Region で一つの AssetManager。
        //      各モデルは、必ず一つの Region に関連付けられる。
        //
        // ※
        // 寿命としては ResourceManager も同様。
        // ただし、ResourceManager は AssetManager から Region をロードするに先駆け、
        // Region の Resource を解決するために用いられる点に注意する。
        //

        // 非同期呼び出しを想定。
        // Region 丸ごと別スレッドでロードするつもり。
        public Region LoadRegion(string uri)
        {
            logger.Info("LoadRegion: {0}", uri);

            var resourceManager = new ResourceManager();

            var resource = resourceManager.Load(uri);

            var assetManager = new AssetManager(serviceProvider);
            assetManager.RegisterLoader(typeof(Region), new RegionLoader(resourceManager));
            assetManager.RegisterLoader(typeof(Image2D), new Image2DLoader(graphicsDevice));
            assetManager.RegisterLoader(typeof(Mesh), new MeshLoader());
            assetManager.RegisterLoader(typeof(Tile), new TileLoader(resourceManager));
            assetManager.RegisterLoader(typeof(TileCatalog), new TileCatalogLoader(resourceManager, graphicsDevice));
            assetManager.RegisterLoader(typeof(Block), new BlockLoader(resourceManager));
            assetManager.RegisterLoader(typeof(BlockCatalog), new BlockCatalogLoader(resourceManager));
            assetManager.RegisterLoader(typeof(IBiome), new BiomeLoader(resourceManager));
            assetManager.RegisterLoader(typeof(IBiomeManager), new BiomeManagerLoader(resourceManager));
            assetManager.RegisterLoader(typeof(BiomeCatalog), new BiomeCatalogLoader(resourceManager));
            assetManager.RegisterLoader(typeof(IChunkProcedure), new ChunkProcedureLoader());
            assetManager.RegisterLoader(typeof(INoiseSource), new NoiseLoader(resourceManager));

            var region = assetManager.Load<Region>(resource);
            region.Initialize(graphicsDevice, assetManager);

            lock (regions)
            {
                regions.Add(region);
            }

            return region;
        }

        public bool RegionExists(ref VectorI3 position)
        {
            Region region;
            return TryGetRegion(ref position, out region);
        }

        public bool TryGetRegion(ref VectorI3 position, out Region region)
        {
            lock (regions)
            {
                foreach (var r in regions)
                {
                    if (r.ContainsPosition(ref position))
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
            foreach (var region in regions) region.Update();
        }

        public void Draw(View view, Projection projection)
        {
            foreach (var region in regions) region.Draw(view, projection);
        }

        public void Close()
        {
            foreach (var region in regions) region.Close();
        }
    }
}
