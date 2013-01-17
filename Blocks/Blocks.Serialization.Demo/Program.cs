#region Using

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Noise;
using Willcraftia.Xna.Framework.Serialization;
using Willcraftia.Xna.Framework.Serialization.Json;
using Willcraftia.Xna.Blocks.Content;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization.Demo
{
    class Program
    {
        #region MockBiome

        class MockBiome : IBiome
        {
            public byte Index { get; set; }

            public INoiseSource DensityNoise
            {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }

            public INoiseSource TerrainNoise
            {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }

            public IResource Resource { get; set; }

            public float GetTemperature(int x, int z) { throw new NotSupportedException(); }

            public float GetHumidity(int x, int z) { throw new NotSupportedException(); }

            public BiomeElement GetBiomeElement(int x, int z) { throw new NotSupportedException(); }
        }

        #endregion

        #region MockNoise

        class MockNoise : INoiseSource
        {
            public float Sample(float x, float y, float z)
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        static string directoryPath;

        static void Main(string[] args)
        {
            #region 開始処理

            directoryPath = Directory.GetCurrentDirectory() + "/Resources";
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            Console.WriteLine("出力先: " + directoryPath);
            Console.WriteLine();

            Console.WriteLine("開始するにはエンター キーを押して下さい...");
            Console.ReadLine();

            #endregion

            #region タイル定義

            Console.WriteLine("タイル定義");
            {
                var tile = new TileDefinition
                {
                    Name = "Default Tile",
                    Texture = "DefaultTile.png",
                    Translucent = false,
                    DiffuseColor = Color.White.PackedValue,
                    EmissiveColor = Color.Black.PackedValue,
                    SpecularColor = Color.Black.PackedValue,
                    SpecularPower = 0
                };
                var jsonResource = SerializeToJson<TileDefinition>("DefaultTile", tile);
                var xmlResource = SerializeToXml<TileDefinition>("DefaultTile", tile);
                var fromJson = DeserializeFromJson<TileDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<TileDefinition>(xmlResource);

                var dirt = new TileDefinition
                {
                    Name = "Dirt Tile",
                    Texture = "DirtTile.png",
                    Translucent = false,
                    DiffuseColor = Color.White.PackedValue,
                    EmissiveColor = Color.Black.PackedValue,
                    SpecularColor = Color.Black.PackedValue,
                    SpecularPower = 0
                };
                jsonResource = SerializeToJson<TileDefinition>("DirtTile", dirt);
                xmlResource = SerializeToXml<TileDefinition>("DirtTile", dirt);
                fromJson = DeserializeFromJson<TileDefinition>(jsonResource);
                fromXml = DeserializeFromXml<TileDefinition>(xmlResource);

                var grassBottom = new TileDefinition
                {
                    Name = "Grass Bottom Tile",
                    Texture = "GrassBottomTile.png",
                    Translucent = false,
                    DiffuseColor = Color.White.PackedValue,
                    EmissiveColor = Color.Black.PackedValue,
                    SpecularColor = Color.Black.PackedValue,
                    SpecularPower = 0
                };
                jsonResource = SerializeToJson<TileDefinition>("GrassBottomTile", grassBottom);
                xmlResource = SerializeToXml<TileDefinition>("GrassBottomTile", grassBottom);
                fromJson = DeserializeFromJson<TileDefinition>(jsonResource);
                fromXml = DeserializeFromXml<TileDefinition>(xmlResource);

                var grassSide = new TileDefinition
                {
                    Name = "Grass Side Tile",
                    Texture = "GrassSideTile.png",
                    Translucent = false,
                    DiffuseColor = Color.White.PackedValue,
                    EmissiveColor = Color.Black.PackedValue,
                    SpecularColor = Color.Black.PackedValue,
                    SpecularPower = 0
                };
                jsonResource = SerializeToJson<TileDefinition>("GrassSideTile", grassSide);
                xmlResource = SerializeToXml<TileDefinition>("GrassSideTile", grassSide);
                fromJson = DeserializeFromJson<TileDefinition>(jsonResource);
                fromXml = DeserializeFromXml<TileDefinition>(xmlResource);

                var grassTop = new TileDefinition
                {
                    Name = "Grass Top Tile",
                    Texture = "GrassTopTile.png",
                    Translucent = false,
                    DiffuseColor = Color.White.PackedValue,
                    EmissiveColor = Color.Black.PackedValue,
                    SpecularColor = Color.Black.PackedValue,
                    SpecularPower = 0
                };
                jsonResource = SerializeToJson<TileDefinition>("GrassTopTile", grassTop);
                xmlResource = SerializeToXml<TileDefinition>("GrassTopTile", grassTop);
                fromJson = DeserializeFromJson<TileDefinition>(jsonResource);
                fromXml = DeserializeFromXml<TileDefinition>(xmlResource);

                var mantle = new TileDefinition
                {
                    Name = "Mantle Tile",
                    Texture = "MantleTile.png",
                    Translucent = false,
                    DiffuseColor = Color.White.PackedValue,
                    EmissiveColor = Color.Black.PackedValue,
                    SpecularColor = Color.Black.PackedValue,
                    SpecularPower = 0
                };
                jsonResource = SerializeToJson<TileDefinition>("MantleTile", mantle);
                xmlResource = SerializeToXml<TileDefinition>("MantleTile", mantle);
                fromJson = DeserializeFromJson<TileDefinition>(jsonResource);
                fromXml = DeserializeFromXml<TileDefinition>(xmlResource);

                var sand = new TileDefinition
                {
                    Name = "Sand Tile",
                    Texture = "SandTile.png",
                    Translucent = false,
                    DiffuseColor = Color.White.PackedValue,
                    EmissiveColor = Color.Black.PackedValue,
                    SpecularColor = Color.Black.PackedValue,
                    SpecularPower = 0
                };
                jsonResource = SerializeToJson<TileDefinition>("SandTile", sand);
                xmlResource = SerializeToXml<TileDefinition>("SandTile", sand);
                fromJson = DeserializeFromJson<TileDefinition>(jsonResource);
                fromXml = DeserializeFromXml<TileDefinition>(xmlResource);

                var snow = new TileDefinition
                {
                    Name = "Snow Tile",
                    Texture = "SnowTile.png",
                    Translucent = false,
                    DiffuseColor = Color.White.PackedValue,
                    EmissiveColor = Color.Black.PackedValue,
                    SpecularColor = Color.Black.PackedValue,
                    SpecularPower = 0
                };
                jsonResource = SerializeToJson<TileDefinition>("SnowTile", snow);
                xmlResource = SerializeToXml<TileDefinition>("SnowTile", snow);
                fromJson = DeserializeFromJson<TileDefinition>(jsonResource);
                fromXml = DeserializeFromXml<TileDefinition>(xmlResource);

                var stone = new TileDefinition
                {
                    Name = "Stone Tile",
                    Texture = "StoneTile.png",
                    Translucent = false,
                    DiffuseColor = Color.White.PackedValue,
                    EmissiveColor = Color.Black.PackedValue,
                    SpecularColor = Color.Black.PackedValue,
                    SpecularPower = 0
                };
                jsonResource = SerializeToJson<TileDefinition>("StoneTile", stone);
                xmlResource = SerializeToXml<TileDefinition>("StoneTile", stone);
                fromJson = DeserializeFromJson<TileDefinition>(jsonResource);
                fromXml = DeserializeFromXml<TileDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region タイル カタログ定義

            Console.WriteLine("タイル カタログ定義");
            {
                var tileCatalog = new TileCatalogDefinition
                {
                    Name = "Default Tile Catalog",
                    Entries = new IndexedUriDefinition[]
                {
                    new IndexedUriDefinition { Index = 1, Uri = "DefaultTile.json" },
                    new IndexedUriDefinition { Index = 2, Uri = "DirtTile.json" },
                    new IndexedUriDefinition { Index = 3, Uri = "GrassBottomTile.json" },
                    new IndexedUriDefinition { Index = 4, Uri = "GrassSideTile.json" },
                    new IndexedUriDefinition { Index = 5, Uri = "GrassTopTile.json" },
                    new IndexedUriDefinition { Index = 6, Uri = "MantleTile.json" },
                    new IndexedUriDefinition { Index = 7, Uri = "SandTile.json" },
                    new IndexedUriDefinition { Index = 8, Uri = "SnowTile.json" },
                    new IndexedUriDefinition { Index = 9, Uri = "StoneTile.json" }
                }
                };
                var jsonResource = SerializeToJson<TileCatalogDefinition>("DefaultTileCatalog", tileCatalog);
                var xmlResource = SerializeToXml<TileCatalogDefinition>("DefaultTileCatalog", tileCatalog);
                var fromJson = DeserializeFromJson<TileCatalogDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<TileCatalogDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region ブロック定義

            Console.WriteLine("ブロック定義");
            {
                var block = new BlockDefinition
                {
                    Name = "Default Block",
                    Mesh = "Cube.json",
                    TopTile = "DefaultTile.json",
                    BottomTile = "DefaultTile.json",
                    FrontTile = "DefaultTile.json",
                    BackTile = "DefaultTile.json",
                    LeftTile = "DefaultTile.json",
                    RightTile = "DefaultTile.json",
                    Fluid = false,
                    ShadowCasting = true,
                    Shape = BlockShape.Cube,
                    Mass = 1,
                    StaticFriction = 0.5f,
                    DynamicFriction = 0.5f,
                    Restitution = 0.5f
                };
                var jsonResource = SerializeToJson<BlockDefinition>("DefaultBlock", block);
                var xmlResource = SerializeToXml<BlockDefinition>("DefaultBlock", block);
                var fromJson = DeserializeFromJson<BlockDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<BlockDefinition>(xmlResource);

                var dirt = new BlockDefinition
                {
                    Name = "Dirt Block",
                    Mesh = "Cube.json",
                    TopTile = "DirtTile.json",
                    BottomTile = "DirtTile.json",
                    FrontTile = "DirtTile.json",
                    BackTile = "DirtTile.json",
                    LeftTile = "DirtTile.json",
                    RightTile = "DirtTile.json",
                    Fluid = false,
                    ShadowCasting = true,
                    Shape = BlockShape.Cube,
                    Mass = 1,
                    StaticFriction = 0.5f,
                    DynamicFriction = 0.5f,
                    Restitution = 0.5f
                };
                jsonResource = SerializeToJson<BlockDefinition>("DirtBlock", dirt);
                xmlResource = SerializeToXml<BlockDefinition>("DirtBlock", dirt);
                fromJson = DeserializeFromJson<BlockDefinition>(jsonResource);
                fromXml = DeserializeFromXml<BlockDefinition>(xmlResource);

                var grass = new BlockDefinition
                {
                    Name = "Grass Block",
                    Mesh = "Cube.json",
                    TopTile = "GrassTopTile.json",
                    BottomTile = "GrassBottomTile.json",
                    FrontTile = "GrassSideTile.json",
                    BackTile = "GrassSideTile.json",
                    LeftTile = "GrassSideTile.json",
                    RightTile = "GrassSideTile.json",
                    Fluid = false,
                    ShadowCasting = true,
                    Shape = BlockShape.Cube,
                    Mass = 1,
                    StaticFriction = 0.5f,
                    DynamicFriction = 0.5f,
                    Restitution = 0.5f
                };
                jsonResource = SerializeToJson<BlockDefinition>("GrassBlock", grass);
                xmlResource = SerializeToXml<BlockDefinition>("GrassBlock", grass);
                fromJson = DeserializeFromJson<BlockDefinition>(jsonResource);
                fromXml = DeserializeFromXml<BlockDefinition>(xmlResource);

                var mantle = new BlockDefinition
                {
                    Name = "Mantle Block",
                    Mesh = "Cube.json",
                    TopTile = "MantleTile.json",
                    BottomTile = "MantleTile.json",
                    FrontTile = "MantleTile.json",
                    BackTile = "MantleTile.json",
                    LeftTile = "MantleTile.json",
                    RightTile = "MantleTile.json",
                    Fluid = false,
                    ShadowCasting = true,
                    Shape = BlockShape.Cube,
                    Mass = 1,
                    StaticFriction = 0.5f,
                    DynamicFriction = 0.5f,
                    Restitution = 0.5f
                };
                jsonResource = SerializeToJson<BlockDefinition>("MantleBlock", mantle);
                xmlResource = SerializeToXml<BlockDefinition>("MantleBlock", mantle);
                fromJson = DeserializeFromJson<BlockDefinition>(jsonResource);
                fromXml = DeserializeFromXml<BlockDefinition>(xmlResource);

                var sand = new BlockDefinition
                {
                    Name = "Sand Block",
                    Mesh = "Cube.json",
                    TopTile = "SandTile.json",
                    BottomTile = "SandTile.json",
                    FrontTile = "SandTile.json",
                    BackTile = "SandTile.json",
                    LeftTile = "SandTile.json",
                    RightTile = "SandTile.json",
                    Fluid = false,
                    ShadowCasting = true,
                    Shape = BlockShape.Cube,
                    Mass = 1,
                    StaticFriction = 0.5f,
                    DynamicFriction = 0.5f,
                    Restitution = 0.5f
                };
                jsonResource = SerializeToJson<BlockDefinition>("SandBlock", sand);
                xmlResource = SerializeToXml<BlockDefinition>("SandBlock", sand);
                fromJson = DeserializeFromJson<BlockDefinition>(jsonResource);
                fromXml = DeserializeFromXml<BlockDefinition>(xmlResource);

                var snow = new BlockDefinition
                {
                    Name = "Snow Block",
                    Mesh = "Cube.json",
                    TopTile = "SnowTile.json",
                    BottomTile = "SnowTile.json",
                    FrontTile = "SnowTile.json",
                    BackTile = "SnowTile.json",
                    LeftTile = "SnowTile.json",
                    RightTile = "SnowTile.json",
                    Fluid = false,
                    ShadowCasting = true,
                    Shape = BlockShape.Cube,
                    Mass = 1,
                    StaticFriction = 0.5f,
                    DynamicFriction = 0.5f,
                    Restitution = 0.5f
                };
                jsonResource = SerializeToJson<BlockDefinition>("SnowBlock", snow);
                xmlResource = SerializeToXml<BlockDefinition>("SnowBlock", snow);
                fromJson = DeserializeFromJson<BlockDefinition>(jsonResource);
                fromXml = DeserializeFromXml<BlockDefinition>(xmlResource);

                var stone = new BlockDefinition
                {
                    Name = "Stone Block",
                    Mesh = "Cube.json",
                    TopTile = "StoneTile.json",
                    BottomTile = "StoneTile.json",
                    FrontTile = "StoneTile.json",
                    BackTile = "StoneTile.json",
                    LeftTile = "StoneTile.json",
                    RightTile = "StoneTile.json",
                    Fluid = false,
                    ShadowCasting = true,
                    Shape = BlockShape.Cube,
                    Mass = 1,
                    StaticFriction = 0.5f,
                    DynamicFriction = 0.5f,
                    Restitution = 0.5f
                };
                jsonResource = SerializeToJson<BlockDefinition>("StoneBlock", stone);
                xmlResource = SerializeToXml<BlockDefinition>("StoneBlock", stone);
                fromJson = DeserializeFromJson<BlockDefinition>(jsonResource);
                fromXml = DeserializeFromXml<BlockDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region ブロック カタログ定義

            Console.WriteLine("ブロック カタログ定義");
            {
                var blockCatalog = new BlockCatalogDefinition
                {
                    Name = "Default Block Catalog",
                    Entries = new IndexedUriDefinition[]
                    {
                        new IndexedUriDefinition { Index = 1, Uri = "DefaultBlock.json" },
                        new IndexedUriDefinition { Index = 2, Uri = "DirtBlock.json" },
                        new IndexedUriDefinition { Index = 3, Uri = "GrassBlock.json" },
                        new IndexedUriDefinition { Index = 4, Uri = "MantleBlock.json" },
                        new IndexedUriDefinition { Index = 5, Uri = "SandBlock.json" },
                        new IndexedUriDefinition { Index = 6, Uri = "SnowBlock.json" },
                        new IndexedUriDefinition { Index = 7, Uri = "StoneBlock.json" }
                    },
                    Dirt = 2,
                    Grass = 3,
                    Mantle = 4,
                    Sand = 5,
                    Snow = 6,
                    Stone = 7
                };
                var jsonResource = SerializeToJson<BlockCatalogDefinition>("DefaultBlockCatalog", blockCatalog);
                var xmlResource = SerializeToXml<BlockCatalogDefinition>("DefaultBlockCatalog", blockCatalog);
                var fromJson = DeserializeFromJson<BlockCatalogDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<BlockCatalogDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region 地形ノイズ コンポーネント

            Console.WriteLine("地形ノイズ コンポーネント");
            {
                // デバッグのし易さのために、各ノイズ インスタンスのコンポーネント名を明示する。
                var componentInfoManager = new ComponentInfoManager(NoiseLoader.ComponentTypeRegistory);
                var builder = new ComponentBundleBuilder(componentInfoManager);

                //------------------------------------------------------------
                //
                // フェード曲線
                //

                // デフォルトでは Perlin.FadeCurve は静的フィールドで共有状態なので、
                // ここで一つだけビルダへ登録しておく。
                builder.Add("DefaultFadeCurve", Perlin.DefaultFadeCurve);

                //------------------------------------------------------------
                //
                // 定数
                //

                var constZero = new Const
                {
                    Name = "Const Zero",
                    Value = 0
                };
                builder.Add("ConstZero", constZero);

                var constOne = new Const
                {
                    Name = "Const One",
                    Value = 1
                };
                builder.Add("ConstOne", constOne);

                //------------------------------------------------------------
                //
                // 低地形状
                //

                var lowlandPerlin = new Perlin
                {
                    Name = "Lowland Perlin",
                    Seed = 100
                };
                builder.Add("LowlandPerlin", lowlandPerlin);

                var lowlandFractal = new Billow
                {
                    Name = "Lowland Fractal",
                    OctaveCount = 2,
                    Source = lowlandPerlin
                };
                builder.Add("LowlandFractal", lowlandFractal);

                var lowlandScaleBias = new ScaleBias
                {
                    Name = "Lowland ScaleBias",
                    Scale = 0.125f,
                    Bias = -0.45f,
                    Source = lowlandFractal
                };
                builder.Add("LowlandScaleBias", lowlandScaleBias);

                // Y のスケールを 0 にすることとは、Y に 0 を指定することと同じ。
                // つまり、出力をハイトマップ上の高さとして扱うということ。
                var lowlandShape = new ScalePoint
                {
                    Name = "Lowland Shape",
                    ScaleY = 0,
                    Source = lowlandScaleBias
                };
                builder.Add("LowlandShape", lowlandShape);

                //------------------------------------------------------------
                //
                // 高地形状
                //

                var highlandPerlin = new Perlin
                {
                    Name = "Highland Perlin",
                    Seed = 200
                };
                builder.Add("HighlandPerlin", highlandPerlin);

                var highlandFractal = new SumFractal
                {
                    Name = "Highland Fractal",
                    OctaveCount = 4,
                    Frequency = 2,
                    Source = highlandPerlin
                };
                builder.Add("HighlandFractal", highlandFractal);

                var highlandShape = new ScalePoint
                {
                    Name = "Highland Shape",
                    ScaleY = 0,
                    Source = highlandFractal
                };
                builder.Add("HighlandShape", highlandShape);

                //------------------------------------------------------------
                //
                // 山地形状
                //
                
                var mountainPerlin = new Perlin
                {
                    Name = "Mountain Perlin",
                    Seed = 300
                };
                builder.Add("MountainPerlin", mountainPerlin);

                var mountainFractal = new RidgedMultifractal
                {
                    Name = "Mountain Fractal",
                    OctaveCount = 2,
                    Source = mountainPerlin
                };
                builder.Add("MountainFractal", mountainFractal);

                var mountainScaleBias = new ScaleBias
                {
                    Name = "Mountain ScaleBias",
                    Scale = 0.5f,
                    Bias = 0.25f,
                    Source = mountainFractal
                };
                builder.Add("MountainScaleBias", mountainScaleBias);

                var mountainShape = new ScalePoint
                {
                    Name = "Mountain Shape",
                    ScaleY = 0,
                    Source = mountainScaleBias
                };
                builder.Add("MountainShape", mountainShape);

                //------------------------------------------------------------
                //
                // 地形形状
                //

                var terrainTypePerlin = new Perlin
                {
                    Name = "Terrain Type Perlin",
                    Seed = 400
                };
                builder.Add("TerrainTypePerlin", terrainTypePerlin);

                var terrainTypeFractal = new SumFractal
                {
                    Name = "Terrain Type Fractal",
                    Frequency = 0.5f,
                    Lacunarity = 0.2f,
                    Source = terrainTypePerlin
                };
                builder.Add("TerrainTypeFractal", terrainTypeFractal);

                var terrainTypeScalePoint = new ScalePoint
                {
                    Name = "Terrain Type ScalePoint",
                    ScaleY = 0,
                    Source = terrainTypeFractal
                };
                builder.Add("TerrainTypeScalePoint", terrainTypeScalePoint);

                // 地形選択ノイズは同時に複数のモジュールから参照されるためキャッシュ。
                var terrainType = new Cache
                {
                    Name = "Terrain Type Cache",
                    Source = terrainTypeScalePoint
                };
                builder.Add("TerrainType", terrainType);

                //------------------------------------------------------------
                //
                // 高地山地ブレンド
                //

                var highlandMountainSelect = new Select
                {
                    Name = "Highland or Mountain Select",
                    LowerSource = highlandShape,
                    LowerBound = 0,
                    UpperSource = mountainShape,
                    UpperBound = 1000,
                    Controller = terrainType,
                    EdgeFalloff = 0.2f
                };
                builder.Add("HighlandMountainSelect", highlandMountainSelect);

                //------------------------------------------------------------
                //
                // 最終地形ブレンド
                //

                var terrainSelect = new Select
                {
                    Name = "Terrain Select",
                    LowerSource = lowlandShape,
                    LowerBound = 0,
                    UpperSource = highlandMountainSelect,
                    UpperBound = 1000,
                    Controller = terrainType,
                    EdgeFalloff = 0.8f
                };
                builder.Add("TerrainSelect", terrainSelect);

                var terrainSelectCache = new Cache
                {
                    Name = "Terrain Select Cache",
                    Source = terrainSelect
                };
                builder.Add("TerrainSelectCache", terrainSelectCache);

                //------------------------------------------------------------
                //
                // 地形密度
                //

                var terrainDensity = new TerrainDensityTest
                {
                    Name = "Terrain Density",
                    Source = terrainSelectCache
                };
                builder.Add("TerrainDensity", terrainDensity);

                //------------------------------------------------------------
                //
                // 洞窟形状
                //

                var cavePerlin = new Perlin
                {
                    Name = "Cave Perlin",
                    Seed = 500
                };
                builder.Add("CavePerlin", cavePerlin);

                var caveShape = new RidgedMultifractal
                {
                    Name = "Cave Shape",
                    OctaveCount = 1,
                    Frequency = 4,
                    Source = cavePerlin
                };
                builder.Add("CaveShape", caveShape);

                var caveAttenuateBias = new ScaleBias
                {
                    Name = "Cave Attenuate ScaleBias",
                    Bias = 0.45f,
                    Source = terrainSelectCache
                };
                builder.Add("CaveAttenuateBias", caveAttenuateBias);

                var caveShapeAttenuate = new Multiply
                {
                    Name = "Cave Shape Attenuate",
                    Source0 = caveShape,
                    Source1 = caveAttenuateBias
                };
                builder.Add("CaveShapeAttenuate", caveShapeAttenuate);

                var cavePerturbPerlin = new Perlin
                {
                    Name = "Cave Perturb Perlin",
                    Seed = 600
                };
                builder.Add("CavePerturbPerlin", cavePerturbPerlin);

                var cavePerturnFractal = new SumFractal
                {
                    Name = "Cave Perturb Fractal",
                    OctaveCount = 6,
                    Frequency = 3,
                    Source = cavePerturbPerlin
                };
                builder.Add("CavePerturnFractal", cavePerturnFractal);

                var cavePerturbScaleBias = new ScaleBias
                {
                    Name = "Cave Perturb ScaleBias",
                    Scale = 0.5f,
                    Source = cavePerturnFractal
                };
                builder.Add("CavePerturbScaleBias", cavePerturbScaleBias);

                var cavePerturb = new Displace
                {
                    Name = "Cave Perturb",
                    DisplaceX = cavePerturbScaleBias,
                    DisplaceY = constZero,
                    DisplaceZ = constZero,
                    Source = caveShapeAttenuate
                };
                builder.Add("CavePerturb", cavePerturb);

                //------------------------------------------------------------
                //
                // 洞窟密度
                //

                var caveDensity = new Select
                {
                    Name = "Cave Density",
                    LowerBound = 0.2f,
                    LowerSource = constOne,
                    UpperBound = 1000,
                    UpperSource = constZero,
                    Controller = cavePerturb
                };
                builder.Add("CaveDensity", caveDensity);

                //------------------------------------------------------------
                //
                // 密度ブレンド
                //

                var finalDensity = new Multiply
                {
                    Name = "Final Density",
                    Source0 = terrainDensity,
                    Source1 = caveDensity
                };
                builder.Add("FinalDensity", finalDensity);

                //------------------------------------------------------------
                //
                // ブロック用座標変換
                //

                // プロシージャはブロック空間座標で XYZ を指定するため、
                // これらをノイズ空間のスケールへ変更。
                //
                // 16 という数値は、チャンク サイズに一致しているものの、
                // チャンク サイズに一致させるべきものではなく、
                // 期待する結果へノイズをスケーリングできれば何でも良い。
                // ただし、ブロック空間座標は int、ノイズ空間は float であるため、
                // 連続性のあるノイズを抽出するには 1 未満の float のスケールとする必要がある。
                //
                // XZ スケールは 16 から 32 辺りが妥当。
                //      小さすぎると微細な高低差が増えすぎる傾向。
                //      大きすぎると高低差が少なくなり過ぎる傾向。
                // Y スケールは 16 以下が妥当。
                //      16 以上は高低差が激しくなり過ぎる傾向。
                var finalScale = new ScalePoint
                {
                    Name = "Final Scale",
                    ScaleX = 1 / 16f,
                    ScaleY = 1 / 16f,
                    ScaleZ = 1 / 16f,
                    Source = finalDensity
                };
                builder.Add("FinalScale", finalScale);

                // 地形の起伏が現れる Y の位置へブロック空間座標を移動。
                // フラクタル ノイズには [-1, 1] を越える値を返すものもあるため、
                // 期待する Y の位置よりも少し下へ移動させるよう補正した方が良い。
                var target = new Displace
                {
                    Name = "Terrain Offset",
                    DisplaceX = constZero,
                    DisplaceY = new Const
                    {
                        Value = -(256 - 16 - 8)
                    },
                    DisplaceZ = constZero,
                    Source = finalScale
                };
                builder.Add("Target", target);

                ComponentBundleDefinition bundle;
                builder.BuildDefinition(out bundle);

                var jsonResource = SerializeToJson<ComponentBundleDefinition>("DefaultTerrainNoise", bundle);
                var xmlResource = SerializeToXml<ComponentBundleDefinition>("DefaultTerrainNoise", bundle);
                var fromJson = DeserializeFromJson<ComponentBundleDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<ComponentBundleDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region 地形ノイズ コンポーネント (シンプル版)

            Console.WriteLine("地形ノイズ コンポーネント (シンプル版)");
            {
                var componentInfoManager = new ComponentInfoManager(NoiseLoader.ComponentTypeRegistory);
                var builder = new ComponentBundleBuilder(componentInfoManager);

                //------------------------------------------------------------
                //
                // フェード曲線
                //

                // デフォルトでは Perlin.FadeCurve は静的フィールドで共有状態なので、
                // ここで一つだけビルダへ登録しておく。
                builder.Add("DefaultFadeCurve", Perlin.DefaultFadeCurve);

                //------------------------------------------------------------
                //
                // 定数
                //

                var constZero = new Const
                {
                    Name = "Const Zero",
                    Value = 0
                };
                builder.Add("ConstZero", constZero);

                var constOne = new Const
                {
                    Name = "Const One",
                    Value = 1
                };
                builder.Add("ConstOne", constOne);

                //------------------------------------------------------------
                //
                // 低地形状
                //

                var terrainPerlin = new Perlin
                {
                    Name = "Terrain Perlin",
                    Seed = 100
                };
                builder.Add("TerrainPerlin", terrainPerlin);

                var terrainFractal = new Billow
                {
                    Name = "Terrain Fractal",
                    OctaveCount = 2,
                    Source = terrainPerlin
                };
                builder.Add("TerrainFractal", terrainFractal);

                var terrainScaleBias = new ScaleBias
                {
                    Name = "Terrain ScaleBias",
                    Scale = 0.125f,
                    Bias = -0.45f,
                    Source = terrainFractal
                };
                builder.Add("TerrainScaleBias", terrainScaleBias);

                // Y のスケールを 0 にすることとは、Y に 0 を指定することと同じ。
                // つまり、出力をハイトマップ上の高さとして扱うということ。
                var terrainShape = new ScalePoint
                {
                    Name = "Terrain Shape",
                    ScaleY = 0,
                    Source = terrainScaleBias
                };
                builder.Add("TerrainShape", terrainShape);

                var terrainShapeCache = new Cache
                {
                    Name = "Terrain Shape Cache",
                    Source = terrainShape
                };
                builder.Add("TerrainShapeCache", terrainShapeCache);

                //------------------------------------------------------------
                //
                // 地形密度
                //

                var terrainDensity = new TerrainDensityTest
                {
                    Name = "Terrain Density",
                    Source = terrainShapeCache
                };
                builder.Add("TerrainDensity", terrainDensity);

                //------------------------------------------------------------
                //
                // 洞窟形状
                //

                var cavePerlin = new Perlin
                {
                    Name = "Cave Perlin",
                    Seed = 500
                };
                builder.Add("CavePerlin", cavePerlin);

                var caveShape = new RidgedMultifractal
                {
                    Name = "Cave Shape",
                    OctaveCount = 1,
                    Frequency = 4,
                    Source = cavePerlin
                };
                builder.Add("CaveShape", caveShape);

                var caveAttenuateBias = new ScaleBias
                {
                    Name = "Cave Attenuate ScaleBias",
                    Bias = 0.45f,
                    Source = terrainShapeCache
                };
                builder.Add("CaveAttenuateBias", caveAttenuateBias);

                var caveShapeAttenuate = new Multiply
                {
                    Name = "Cave Shape Attenuate",
                    Source0 = caveShape,
                    Source1 = caveAttenuateBias
                };
                builder.Add("CaveShapeAttenuate", caveShapeAttenuate);

                var cavePerturbPerlin = new Perlin
                {
                    Name = "Cave Perturb Perlin",
                    Seed = 600
                };
                builder.Add("CavePerturbPerlin", cavePerturbPerlin);

                var cavePerturnFractal = new SumFractal
                {
                    Name = "Cave Perturb Fractal",
                    OctaveCount = 6,
                    Frequency = 3,
                    Source = cavePerturbPerlin
                };
                builder.Add("CavePerturnFractal", cavePerturnFractal);

                var cavePerturbScaleBias = new ScaleBias
                {
                    Name = "Cave Perturb ScaleBias",
                    Scale = 0.5f,
                    Source = cavePerturnFractal
                };
                builder.Add("CavePerturbScaleBias", cavePerturbScaleBias);

                var cavePerturb = new Displace
                {
                    Name = "Cave Perturb",
                    DisplaceX = cavePerturbScaleBias,
                    DisplaceY = constZero,
                    DisplaceZ = constZero,
                    Source = caveShapeAttenuate
                };
                builder.Add("CavePerturb", cavePerturb);

                //------------------------------------------------------------
                //
                // 洞窟密度
                //

                var caveDensity = new Select
                {
                    Name = "Cave Density",
                    LowerBound = 0.2f,
                    LowerSource = constOne,
                    UpperBound = 1000,
                    UpperSource = constZero,
                    Controller = cavePerturb
                };
                builder.Add("CaveDensity", caveDensity);

                //------------------------------------------------------------
                //
                // 密度ブレンド
                //

                var finalDensity = new Multiply
                {
                    Name = "Final Density",
                    Source0 = terrainDensity,
                    Source1 = caveDensity
                };
                builder.Add("FinalDensity", finalDensity);

                //------------------------------------------------------------
                //
                // ブロック用座標変換
                //

                // プロシージャはブロック空間座標で XYZ を指定するため、
                // これらをノイズ空間のスケールへ変更。
                //
                // 16 という数値は、チャンク サイズに一致しているものの、
                // チャンク サイズに一致させるべきものではなく、
                // 期待する結果へノイズをスケーリングできれば何でも良い。
                // ただし、ブロック空間座標は int、ノイズ空間は float であるため、
                // 連続性のあるノイズを抽出するには 1 未満の float のスケールとする必要がある。
                //
                // XZ スケールは 64 辺りが妥当。
                // Y スケールは 64 以上が妥当。
                //      64 未満は高低差が少なすぎる傾向。
                var finalScale = new ScalePoint
                {
                    Name = "Final Scale",
                    ScaleX = 1 / 64f,
                    ScaleY = 1 / 128f,
                    ScaleZ = 1 / 64f,
                    Source = finalDensity
                };
                builder.Add("FinalScale", finalScale);

                // 地形の起伏が現れる Y の位置へブロック空間座標を移動。
                var target = new Displace
                {
                    Name = "Terrain Offset",
                    DisplaceX = constZero,
                    DisplaceY = new Const
                    {
                        Value = -256
                    },
                    DisplaceZ = constZero,
                    Source = finalScale
                };
                builder.Add("Target", target);

                ComponentBundleDefinition bundle;
                builder.BuildDefinition(out bundle);

                var jsonResource = SerializeToJson<ComponentBundleDefinition>("SimpleTerrainNoise", bundle);
                var xmlResource = SerializeToXml<ComponentBundleDefinition>("SimpleTerrainNoise", bundle);
                var fromJson = DeserializeFromJson<ComponentBundleDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<ComponentBundleDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region バイオーム コンポーネント

            Console.WriteLine("バイオーム コンポーネント");
            {
                // デバッグのし易さのために、各ノイズ インスタンスのコンポーネント名を明示する。
                var componentInfoManager = new ComponentInfoManager(BiomeLoader.ComponentTypeRegistory);
                var builder = new ComponentBundleBuilder(componentInfoManager);

                // デフォルトでは Perlin.FadeCurve は静的フィールドで共有状態なので、
                // ここで一つだけビルダへ登録しておく。
                builder.Add("DefaultFadeCurve", Perlin.DefaultFadeCurve);

                //------------------------------------------------------------
                //
                // 湿度
                //

                // humidityPerlin
                var humidityPerlin = new Perlin
                {
                    Name = "Humidity Perlin",
                    Seed = 100
                };
                // humidityFractal
                var humidityFractal = new SumFractal
                {
                    Name = "Humidity Fractal",
                    Source = humidityPerlin
                };
                // humidity
                var humidity = new ScaleBias
                {
                    Name = "Humidity",
                    Scale = 0.5f,
                    Bias = 0.5f,
                    Source = humidityFractal
                };
                builder.Add("HumidityPerlin", humidityPerlin);
                builder.Add("HumidityFractal", humidityFractal);
                builder.Add("Humidity", humidity);

                //------------------------------------------------------------
                //
                // 気温
                //

                // temperaturePerlin
                var temperaturePerlin = new Perlin
                {
                    Name = "Temperature Perlin",
                    Seed = 200
                };
                // temperatureFractal
                var temperatureFractal = new SumFractal
                {
                    Name = "Temperature Fractal",
                    Source = temperaturePerlin
                };
                // temperature
                var temperature = new ScaleBias
                {
                    Name = "Temperature",
                    Scale = 0.5f,
                    Bias = 0.5f,
                    Source = temperatureFractal
                };
                builder.Add("TemperaturePerlin", temperaturePerlin);
                builder.Add("TemperatureFractal", temperatureFractal);
                builder.Add("Temperature", temperature);

                //------------------------------------------------------------
                //
                // バイオーム
                //

                // biome
                var biome = new DefaultBiome
                {
                    Name = "Default Biome",
                    HumidityNoise = humidity,
                    TemperatureNoise = temperature,
                    TerrainNoise = new MockNoise()
                };
                builder.Add("Target", biome);
                builder.AddExternalReference(biome.TerrainNoise, "title:Resources/DefaultTerrainNoise.json");

                ComponentBundleDefinition biomeBundle;
                builder.BuildDefinition(out biomeBundle);

                var jsonResource = SerializeToJson<ComponentBundleDefinition>("DefaultBiome", biomeBundle);
                var xmlResource = SerializeToXml<ComponentBundleDefinition>("DefaultBiome", biomeBundle);
                var fromJson = DeserializeFromJson<ComponentBundleDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<ComponentBundleDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region バイオーム カタログ定義

            Console.WriteLine("バイオーム カタログ定義");
            {
                var biomeCatalog = new BiomeCatalogDefinition
                {
                    Name = "Default Biome Catalog",
                    Entries = new IndexedUriDefinition[]
                    {
                        new IndexedUriDefinition
                        {
                            Index = 0, Uri = "DefaultBiome.json"
                        }
                    }
                };
                var jsonResource = SerializeToJson<BiomeCatalogDefinition>("DefaultBiomeCatalog", biomeCatalog);
                var xmlResource = SerializeToXml<BiomeCatalogDefinition>("DefaultBiomeCatalog", biomeCatalog);
                var fromJson = DeserializeFromJson<BiomeCatalogDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<BiomeCatalogDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region 単一バイオーム マネージャ コンポーネント

            Console.WriteLine("単一バイオーム マネージャ コンポーネント");
            {
                var biomeManager = new SingleBiomeManager
                {
                    Name = "Default Biome Manager",
                    Biome = new MockBiome()
                };

                var componentInfoManager = new ComponentInfoManager(BiomeManagerLoader.ComponentTypeRegistory);
                var builder = new ComponentBundleBuilder(componentInfoManager);
                builder.AddExternalReference(biomeManager.Biome, "DefaultBiome.json");
                builder.Add("Target", biomeManager);

                ComponentBundleDefinition bundle;
                builder.BuildDefinition(out bundle);

                var jsonResource = SerializeToJson<ComponentBundleDefinition>("DefaultBiomeManager", bundle);
                var xmlResource = SerializeToXml<ComponentBundleDefinition>("DefaultBiomeManager", bundle);
                var fromJson = DeserializeFromJson<ComponentBundleDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<ComponentBundleDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region 平坦地形生成コンポーネント
            
            Console.WriteLine("平坦地形生成コンポーネント");
            {
                var procedure = new FlatTerrainProcedure
                {
                    Name = "Default Flat Terrain Procedure",
                    Height = 156
                };

                var componentInfoManager = new ComponentInfoManager(ChunkProcedureLoader.ComponentTypeRegistory);
                var builder = new ComponentBundleBuilder(componentInfoManager);
                builder.Add("Target", procedure);

                ComponentBundleDefinition bundle;
                builder.BuildDefinition(out bundle);

                var jsonResource = SerializeToJson<ComponentBundleDefinition>("DefaultFlatTerrainProcedure", bundle);
                var xmlResource = SerializeToXml<ComponentBundleDefinition>("DefaultFlatTerrainProcedure", bundle);
                var fromJson = DeserializeFromJson<ComponentBundleDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<ComponentBundleDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region デフォルト地形生成コンポーネント

            Console.WriteLine("デフォルト地形生成コンポーネント");
            {
                var procedure = new DefaultTerrainProcedure
                {
                    Name = "Default Noise Terrain Procedure",
                };

                var componentInfoManager = new ComponentInfoManager(ChunkProcedureLoader.ComponentTypeRegistory);
                var builder = new ComponentBundleBuilder(componentInfoManager);
                builder.Add("Target", procedure);

                ComponentBundleDefinition bundle;
                builder.BuildDefinition(out bundle);

                var jsonResource = SerializeToJson<ComponentBundleDefinition>("DefaultTerrainProcedure", bundle);
                var xmlResource = SerializeToXml<ComponentBundleDefinition>("DefaultTerrainProcedure", bundle);
                var fromJson = DeserializeFromJson<ComponentBundleDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<ComponentBundleDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region リージョン定義

            Console.WriteLine("リージョン定義");
            {
                var region = new RegionDefinition
                {
                    Name = "Default Region",
                    Bounds = new BoundingBoxI
                    {
                        Min = new VectorI3(-128, 0, -128),
                        Max = new VectorI3(128, 16, 128)
                    },
                    TileCatalog = "DefaultTileCatalog.json",
                    BlockCatalog = "DefaultBlockCatalog.json",
                    BiomeManager = "DefaultBiomeManager.json",
                    ChunkProcedures = new string[]
                    {
                        "DefaultTerrainProcedure.json"
                    },
                    ChunkStore = ChunkStoreTypes.None
                };
                var jsonResource = SerializeToJson<RegionDefinition>("DefaultRegion", region);
                var xmlResource = SerializeToXml<RegionDefinition>("DefaultRegion", region);
                var fromJson = DeserializeFromJson<RegionDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<RegionDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region スカイ スフィア定義

            Console.WriteLine("スカイ スフィア定義");
            {
                var skySphere = new SkySphereDefinition
                {
                    SunVisible = true,
                    SunThreshold = 0.999f
                };
                var jsonResource = SerializeToJson<SkySphereDefinition>("DefaultSkySphere", skySphere);
                var xmlResource = SerializeToXml<SkySphereDefinition>("DefaultSkySphere", skySphere);
                var fromJson = DeserializeFromJson<SkySphereDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<SkySphereDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region グラフィックス設定定義

            Console.WriteLine("グラフィックス設定定義");
            {
                var settings = new GraphicsSettingsDefinition
                {
                    ShadowMapEnabled = true,
                    ShadowMap = new ShadowMapSettingsDefinition
                    {
                        Technique = ShadowMap.Techniques.Basic,
                        Size = 2048,
                        DepthBias = 0.0005f,
                        FarPlaneDistance = 8 * 16,
                        SplitCount = 3,
                        SplitLambda = 0.5f,
                        VsmBlur = new BlurSettingsDefinition
                        {
                            Radius = 1,
                            Amount = 1
                        }
                    },
                    SssmEnabled = false,
                    Sssm = new SssmSettingsDefinition
                    {
                        MapScale = 0.5f,
                        BlurEnabled = true,
                        Blur = new BlurSettingsDefinition
                        {
                            Radius = 1,
                            Amount = 1
                        }
                    },
                    SsaoEnabled = false,
                    Ssao = new SsaoSettingsDefinition
                    {
                        MapScale = 1,
                        Blur = new BlurSettingsDefinition
                        {
                            Radius = 1,
                            Amount = 2
                        }
                    },
                    EdgeEnabled = false,
                    Edge = new EdgeSettingsDefinition
                    {
                        MapScale = 1,
                    },
                    BloomEnabled = false,
                    Bloom = new BloomSettingsDefinition
                    {
                        MapScale = 0.25f,
                        Blur = new BlurSettingsDefinition
                        {
                            Radius = 1,
                            Amount = 4
                        }
                    },
                    DofEnabled = true,
                    Dof = new DofSettingsDefinition
                    {
                        MapScale = 0.5f,
                        Blur = new BlurSettingsDefinition
                        {
                            Radius = 1,
                            Amount = 1
                        }
                    },
                    ColorOverlapEnabled = false,
                    MonochromeEnabled = false,
                    ScanlineEnabled = false,
                    LensFlareEnabled = true,
                };
                var jsonResource = SerializeToJson<GraphicsSettingsDefinition>("GraphicsSettings", settings);
                var xmlResource = SerializeToXml<GraphicsSettingsDefinition>("GraphicsSettings", settings);
                var fromJson = DeserializeFromJson<GraphicsSettingsDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<GraphicsSettingsDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region チャンク定義

            Console.WriteLine("チャンク定義");
            {
                var settings = new ChunkSettingsDefinition
                {
                    ChunkSize = new VectorI3(16),
                    MeshUpdateSearchCapacity = 100,
                    VerticesBuilderCount = 5,
                    MinActiveRange = 10,
                    MaxActiveRange = 12,
                    ChunkPoolMaxCapacity = 0,
                    ClusterSize = new VectorI3(8),
                    InitialActiveChunkCapacity = 5000,
                    InitialActiveClusterCapacity = 50,
                    InitialActivationCapacity = 3,
                    InitialPassivationCapacity = 1000,
                    ActivationSearchCapacity = 100,
                    PassivationSearchCapacity = 200,
                    ActivationTaskQueueSlotCount = 3,
                    PassivationTaskQueueSlotCount = 50
                };
                var jsonResource = SerializeToJson<ChunkSettingsDefinition>("ChunkSettings", settings);
                var xmlResource = SerializeToXml<ChunkSettingsDefinition>("ChunkSettings", settings);
                var fromJson = DeserializeFromJson<ChunkSettingsDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<ChunkSettingsDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region シーン設定定義

            Console.WriteLine("シーン設定定義");
            {
                var sceneSettings = new SceneSettingsDefinition
                {
                    MidnightSunDirection = new Vector3(0, -1, 1),
                    MidnightMoonDirection = new Vector3(0, 1, 1),
                    ShadowColor = Color.DimGray.ToVector3(),
                    SunlightDiffuseColor = Vector3.One,
                    SunlightSpecularColor = Vector3.Zero,
                    SunlightEnabled = true,
                    MoonlightDiffuseColor = new Vector3(0.5f),
                    MoonlightSpecularColor = Vector3.Zero,
                    MoonlightEnabled = false,
                    SkyColors = new TimeColorDefinition[]
                    {
                        new TimeColorDefinition { Time = 0, Color = Color.Black.ToVector3() },
                        new TimeColorDefinition { Time = 0.15f, Color = Color.Black.ToVector3() },
                        new TimeColorDefinition { Time = 0.25f, Color = Color.CornflowerBlue.ToVector3() },
                        new TimeColorDefinition { Time = 0.5f, Color = Color.CornflowerBlue.ToVector3() },
                        new TimeColorDefinition { Time = 0.75f, Color = Color.CornflowerBlue.ToVector3() },
                        new TimeColorDefinition { Time = 0.84f, Color = Color.Black.ToVector3() },
                        new TimeColorDefinition { Time = 1, Color = Color.Black.ToVector3() }
                    },
                    AmbientLightColors = new TimeColorDefinition[]
                    {
                        new TimeColorDefinition { Time = 0, Color = new Vector3(0.1f) },
                        new TimeColorDefinition { Time = 0.15f, Color = new Vector3(0.1f) },
                        new TimeColorDefinition { Time = 0.5f, Color = new Vector3(1) },
                        new TimeColorDefinition { Time = 0.84f, Color = new Vector3(0.1f) },
                        new TimeColorDefinition { Time = 1, Color = new Vector3(0.1f) }
                    },
                    InitialFogEnabled = true,
                    InitialFogStartRate = 0.7f,
                    InitialFogEndRate = 0.9f,
                    SecondsPerDay = 20f,
                    TimeStopped = false,
                    FixedSecondsPerDay = 8.3f
                };
                var jsonResource = SerializeToJson<SceneSettingsDefinition>("SceneSettings", sceneSettings);
                var xmlResource = SerializeToXml<SceneSettingsDefinition>("SceneSettings", sceneSettings);
                var fromJson = DeserializeFromJson<SceneSettingsDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<SceneSettingsDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region 立方体メッシュ定義

            Console.WriteLine("立方体メッシュ定義");
            {
                var cubeMesh = CreateCubeMeshDefinition();

                var jsonResource = SerializeToJson<MeshDefinition>("Cube", cubeMesh);
                var xmlResource = SerializeToXml<MeshDefinition>("Cube", cubeMesh);
                var fromJson = DeserializeFromJson<MeshDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<MeshDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region 降雪パーティクル定義

            Console.WriteLine("降雪パーティクル");
            {
                var definition = new ParticleSettingsDefinition
                {
                    Name = "Default Snow Particle",
                    MaxParticles = 4000,
                    Duration = 5,
                    DurationRandomness = 0,
                    MinHorizontalVelocity = 0,
                    MaxHorizontalVelocity = 0,
                    MinVerticalVelocity = -10,
                    MaxVerticalVelocity = -10,
                    Gravity = new Vector3(-1, -1, 0),
                    EndVelocity = 1,
                    MinColor = Color.White.ToVector4(),
                    MaxColor = Color.White.ToVector4(),
                    MinRotateSpeed = 0,
                    MaxRotateSpeed = 0,
                    MinStartSize = 0.5f,
                    MaxStartSize = 0.5f,
                    MinEndSize = 0.2f,
                    MaxEndSize = 0.2f,
                    Texture = "title:Resources/DefaultSnowParticle.png",
                    BlendState = BlendState.AlphaBlend
                };
                var jsonResource = SerializeToJson<ParticleSettingsDefinition>("DefaultSnowParticle", definition);
                var xmlResource = SerializeToXml<ParticleSettingsDefinition>("DefaultSnowParticle", definition);
                var fromJson = DeserializeFromJson<ParticleSettingsDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<ParticleSettingsDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region 降雨パーティクル定義

            Console.WriteLine("降雨パーティクル定義");
            {
                var definition = new ParticleSettingsDefinition
                {
                    Name = "Default Rain Particle",
                    MaxParticles = 8000,
                    Duration = 2,
                    DurationRandomness = 0,
                    MinHorizontalVelocity = 0,
                    MaxHorizontalVelocity = 0,
                    MinVerticalVelocity = -50,
                    MaxVerticalVelocity = -50,
                    Gravity = new Vector3(-1, -1, 0),
                    EndVelocity = 1,
                    MinColor = Color.White.ToVector4(),
                    MaxColor = Color.White.ToVector4(),
                    MinRotateSpeed = 0,
                    MaxRotateSpeed = 0,
                    MinStartSize = 0.5f,
                    MaxStartSize = 0.5f,
                    MinEndSize = 0.5f,
                    MaxEndSize = 0.5f,
                    Texture = "title:Resources/DefaultRainParticle.png",
                    BlendState = BlendState.AlphaBlend
                };
                var jsonResource = SerializeToJson<ParticleSettingsDefinition>("DefaultRainParticle", definition);
                var xmlResource = SerializeToXml<ParticleSettingsDefinition>("DefaultRainParticle", definition);
                var fromJson = DeserializeFromJson<ParticleSettingsDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<ParticleSettingsDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region 終了処理

            Console.WriteLine("終了するにはエンター キーを押して下さい...");
            Console.ReadLine();

            #endregion
        }

        #region シリアライゼーション/デシリアライゼーション補助メソッド

        static IResource SerializeToJson<T>(string baseFileName, T instance)
        {
            var serializer = new JsonSerializerAdapter(typeof(T));
            serializer.JsonSerializer.Formatting = Newtonsoft.Json.Formatting.Indented;

            var resource = FileResourceLoader.Instance.LoadResource("file:///" + directoryPath + "/" + baseFileName + ".json");
            using (var stream = resource.Create())
            {
                serializer.Serialize(stream, instance);
            }

            Console.WriteLine("シリアライズ: " + Path.GetFileName(resource.AbsoluteUri));
            return resource;
        }

        static IResource SerializeToXml<T>(string baseFileName, T instance)
        {
            var serializer = new XmlSerializerAdapter(typeof(T));
            serializer.WriterSettings.Indent = true;
            serializer.WriterSettings.IndentChars = "\t";
            serializer.WriterSettings.OmitXmlDeclaration = true;

            var resource = FileResourceLoader.Instance.LoadResource("file:///" + directoryPath + "/" + baseFileName + ".xml");
            using (var stream = resource.Create())
            {
                serializer.Serialize(stream, instance);
            }

            Console.WriteLine("シリアライズ: " + Path.GetFileName(resource.AbsoluteUri));
            return resource;
        }

        static T DeserializeFromJson<T>(IResource resource)
        {
            var serializer = new JsonSerializerAdapter(typeof(T));
            serializer.JsonSerializer.Formatting = Newtonsoft.Json.Formatting.Indented;

            T result;
            using (var stream = resource.Open())
            {
                result = (T) serializer.Deserialize(stream, null);
            }
            Console.WriteLine("デシリアライズ: " + Path.GetFileName(resource.AbsoluteUri));
            return result;
        }

        static T DeserializeFromXml<T>(IResource resource)
        {
            var serializer = new XmlSerializerAdapter(typeof(T));

            T result;
            using (var stream = resource.Open())
            {
                result = (T) serializer.Deserialize(stream, null);
            }
            Console.WriteLine("デシリアライズ: " + Path.GetFileName(resource.AbsoluteUri));
            return result;
        }

        #endregion

        #region 立方体メッシュ生成補助メソッド

        static MeshDefinition CreateCubeMeshDefinition()
        {
            Vector3[] normals =
            {
                new Vector3( 0,  1,  0),
                new Vector3( 0, -1,  0),
                new Vector3( 0,  0,  1),
                new Vector3( 0,  0, -1),
                new Vector3(-1,  0,  0),
                new Vector3( 1,  0,  0)
            };

            ushort[] indices = { 0, 1, 2, 0, 2, 3 };

            var mesh = new MeshDefinition();
            mesh.Name = "Default Cube";
            CreateMeshPartDefinition(ref normals[0], out mesh.Top);
            CreateMeshPartDefinition(ref normals[1], out mesh.Bottom);
            CreateMeshPartDefinition(ref normals[2], out mesh.Front);
            CreateMeshPartDefinition(ref normals[3], out mesh.Back);
            CreateMeshPartDefinition(ref normals[4], out mesh.Left);
            CreateMeshPartDefinition(ref normals[5], out mesh.Right);

            // Front と Back のみ UV 座標を調整。

            mesh.Front.Vertices[0].TextureCoordinate = new Vector2(0, 1);
            mesh.Front.Vertices[1].TextureCoordinate = new Vector2(1, 1);
            mesh.Front.Vertices[2].TextureCoordinate = new Vector2(1, 0);
            mesh.Front.Vertices[3].TextureCoordinate = new Vector2(0, 0);

            mesh.Back.Vertices[0].TextureCoordinate = new Vector2(1, 0);
            mesh.Back.Vertices[1].TextureCoordinate = new Vector2(0, 0);
            mesh.Back.Vertices[2].TextureCoordinate = new Vector2(0, 1);
            mesh.Back.Vertices[3].TextureCoordinate = new Vector2(1, 1);

            return mesh;
        }

        static void CreateMeshPartDefinition(ref Vector3 normal, out MeshPartDefinition result)
        {
            result = new MeshPartDefinition
            {
                Vertices = CreateVertexPositionNormalTexture(ref normal),
                Indices = new ushort[] { 0, 1, 2, 0, 2, 3 }
            };
        }

        static VertexPositionNormalTexture[] CreateVertexPositionNormalTexture(ref Vector3 normal)
        {
            var vertices = new VertexPositionNormalTexture[4];

            var side1 = new Vector3(normal.Y, normal.Z, normal.X);
            var side2 = Vector3.Cross(normal, side1);
            
            vertices[0].Position = (normal - side1 - side2) * 0.5f;
            vertices[1].Position = (normal - side1 + side2) * 0.5f;
            vertices[2].Position = (normal + side1 + side2) * 0.5f;
            vertices[3].Position = (normal + side1 - side2) * 0.5f;

            vertices[0].Normal = normal;
            vertices[1].Normal = normal;
            vertices[2].Normal = normal;
            vertices[3].Normal = normal;

            vertices[0].TextureCoordinate = new Vector2(0, 0);
            vertices[1].TextureCoordinate = new Vector2(0, 1);
            vertices[2].TextureCoordinate = new Vector2(1, 1);
            vertices[3].TextureCoordinate = new Vector2(1, 0);

            return vertices;
        }

        #endregion
    }
}
