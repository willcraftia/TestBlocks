#region Using

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Component;
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
            #region Setup

            //================================================================
            // Setup

            directoryPath = Directory.GetCurrentDirectory() + "/Resources";
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            Console.WriteLine("Output: " + directoryPath);
            Console.WriteLine();

            Console.WriteLine("Press Enter to serialize/deserialize resources.");
            Console.ReadLine();

            #endregion

            #region TileDefinition

            //================================================================
            // TileDefinition

            Console.WriteLine("TileDefinition");
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

            #region TileCatalogDefinition

            //================================================================
            // TileCatalogDefinition

            Console.WriteLine("TileCatalogDefinition");
            {
                var tileCatalog = new TileCatalogDefinition
                {
                    Name = "Default Tile Catalog",
                    Entries = new IndexedUriDefinition[]
                {
                    new IndexedUriDefinition { Index = 0, Uri = "DefaultTile.json" },
                    new IndexedUriDefinition { Index = 1, Uri = "DirtTile.json" },
                    new IndexedUriDefinition { Index = 2, Uri = "GrassBottomTile.json" },
                    new IndexedUriDefinition { Index = 3, Uri = "GrassSideTile.json" },
                    new IndexedUriDefinition { Index = 4, Uri = "GrassTopTile.json" },
                    new IndexedUriDefinition { Index = 5, Uri = "MantleTile.json" },
                    new IndexedUriDefinition { Index = 6, Uri = "SandTile.json" },
                    new IndexedUriDefinition { Index = 7, Uri = "SnowTile.json" },
                    new IndexedUriDefinition { Index = 8, Uri = "StoneTile.json" }
                }
                };
                var jsonResource = SerializeToJson<TileCatalogDefinition>("DefaultTileCatalog", tileCatalog);
                var xmlResource = SerializeToXml<TileCatalogDefinition>("DefaultTileCatalog", tileCatalog);
                var fromJson = DeserializeFromJson<TileCatalogDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<TileCatalogDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region BlockDefinition

            //================================================================
            // BlockDefinition

            Console.WriteLine("BlockDefinition");
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

            #region BlockCatalogDefinition

            //================================================================
            // BlockCatalogDefinition

            Console.WriteLine("BlockCatalogDefinition");
            {
                var blockCatalog = new BlockCatalogDefinition
                {
                    Name = "Default Block Catalog",
                    Entries = new IndexedUriDefinition[]
                    {
                        new IndexedUriDefinition { Index = 0, Uri = "DefaultBlock.json" },
                        new IndexedUriDefinition { Index = 1, Uri = "DirtBlock.json" },
                        new IndexedUriDefinition { Index = 2, Uri = "GrassBlock.json" },
                        new IndexedUriDefinition { Index = 3, Uri = "MantleBlock.json" },
                        new IndexedUriDefinition { Index = 4, Uri = "SandBlock.json" },
                        new IndexedUriDefinition { Index = 5, Uri = "SnowBlock.json" },
                        new IndexedUriDefinition { Index = 6, Uri = "StoneBlock.json" }
                    },
                    Dirt = 1,
                    Grass = 2,
                    Mantle = 3,
                    Sand = 4,
                    Snow = 5,
                    Stone = 6
                };
                var jsonResource = SerializeToJson<BlockCatalogDefinition>("DefaultBlockCatalog", blockCatalog);
                var xmlResource = SerializeToXml<BlockCatalogDefinition>("DefaultBlockCatalog", blockCatalog);
                var fromJson = DeserializeFromJson<BlockCatalogDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<BlockCatalogDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region DensityNoise (INoiseSource) (ComponentBundleDefinition)

            //================================================================
            // TerrainNoise (INoiseSource) (ComponentBundleDefinition)

            Console.WriteLine("DensityNoise (INoiseSource) (ComponentBundleDefinition)");
            {
                // density
                var density = new GradientDensity();

                var componentInfoManager = new ComponentInfoManager(NoiseLoader.ComponentTypeRegistory);
                var builder = new ComponentBundleBuilder(componentInfoManager);
                builder.Add("Target", density);

                ComponentBundleDefinition biomeBundle;
                builder.BuildDefinition(out biomeBundle);

                var jsonResource = SerializeToJson<ComponentBundleDefinition>("DefaultDensityNoise", biomeBundle);
                var xmlResource = SerializeToXml<ComponentBundleDefinition>("DefaultDensityNoise", biomeBundle);
                var fromJson = DeserializeFromJson<ComponentBundleDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<ComponentBundleDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region TerrainNoise (INoiseSource) (ComponentBundleDefinition)

            //================================================================
            // TerrainNoise (INoiseSource) (ComponentBundleDefinition)

            Console.WriteLine("TerrainNoise (INoiseSource) (ComponentBundleDefinition)");
            {
                // lowlandShape
                var lowlandShape = new ScalePoint
                {
                    Name = "Lowland Shape",
                    ScaleY = 0,
                    Source = new ScaleBias
                    {
                        Name = "Lowland ScaleBias",
                        Scale = 0.125f,
                        Bias = -0.75f,
                        Source = new Billow
                        {
                            Name = "Lowland Fractal",
                            OctaveCount = 2,
                            Source = new Perlin
                            {
                                Name = "Lowland Perlin",
                                Seed = 100
                            }
                        }
                    }
                };
                // highlandShape
                var highlandShape = new ScalePoint
                {
                    Name = "Highland Shape",
                    ScaleY = 0,
                    Source = new SumFractal
                    {
                        Name = "Highland Fractal",
                        OctaveCount = 4,
                        Frequency = 2,
                        Source = new Perlin
                        {
                            Name = "Highland Perlin",
                            Seed = 200
                        }
                    }
                };
                // mountainShape
                var mountainShape = new ScalePoint
                {
                    Name = "Mountain Shape",
                    ScaleY = 0,
                    Source = new ScaleBias
                    {
                        Name = "Mountain ScaleBias",
                        Scale = 0.5f,
                        Bias = 0.25f,
                        Source = new RidgedMultifractal
                        {
                            Name = "Mountain Fractal",
                            Source = new Perlin
                            {
                                Name = "Mountain Perlin",
                                Seed = 300
                            }
                        }
                    }
                };
                // terrainType
                var terrainType = new Cache
                {
                    Name = "Terrain Type Cache",
                    Source = new ScalePoint
                    {
                        Name = "Terrain Type ScalePoint",
                        ScaleY = 0,
                        Source = new SumFractal
                        {
                            Name = "Terrain Type Fractal",
                            Frequency = 0.5f,
                            Lacunarity = 0.25f,
                            Source = new Perlin
                            {
                                Name = "Terrain Type Perlin",
                                Seed = 400
                            }
                        }
                    }
                };
                // highlandMountainSelect
                var highlandMountainSelect = new Select
                {
                    Name = "Highland or Mountain Select",
                    LowerSource = highlandShape,
                    LowerBound = 0,
                    UpperSource = mountainShape,
                    UpperBound = 1000,
                    Controller = terrainType,
                    EdgeFalloff = 0.125f
                };
                // terrain
                var terrain = new Select
                {
                    Name = "Final Terrain Select",
                    LowerSource = lowlandShape,
                    LowerBound = 0,
                    UpperSource = highlandMountainSelect,
                    UpperBound = 1000,
                    Controller = terrainType,
                    EdgeFalloff = 0.125f
                };
                //// density
                //var density = new GradientDensity();
                //// terrain
                //var terrain = new Select
                //{
                //    LowerSource = new Const { Value = 0 },
                //    LowerBound = 0.25f,
                //    UpperSource = new Const { Value = 1 },
                //    UpperBound = 1000,
                //    Controller = new Displace
                //    {
                //        DisplaceX = new Const { Value = 0 },
                //        DisplaceY = terrainSelect,
                //        DisplaceZ = new Const { Value = 0 },
                //        Source = density
                //    }
                //};

                var componentInfoManager = new ComponentInfoManager(NoiseLoader.ComponentTypeRegistory);
                var builder = new ComponentBundleBuilder(componentInfoManager);
                builder.Add("Target", terrain);

                ComponentBundleDefinition biomeBundle;
                builder.BuildDefinition(out biomeBundle);

                var jsonResource = SerializeToJson<ComponentBundleDefinition>("DefaultTerrainNoise", biomeBundle);
                var xmlResource = SerializeToXml<ComponentBundleDefinition>("DefaultTerrainNoise", biomeBundle);
                var fromJson = DeserializeFromJson<ComponentBundleDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<ComponentBundleDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region DefaultBiome (ComponentBundleDefinition)

            //================================================================
            // DefaultBiome (ComponentBundleDefinition)

            Console.WriteLine("DefaultBiome (ComponentBundleDefinition)");
            {
                var biome = new DefaultBiome
                {
                    Name = "Default Biome",
                    HumidityNoise = new ScaleBias
                    {
                        Name = "Humidity ScaleBias",
                        Scale = 0.5f,
                        Bias = 0.5f,
                        Source = new SumFractal
                        {
                            Name = "Humidity Fractal",
                            Source = new Perlin
                            {
                                Name = "Humidity Perlin",
                                Seed = 100
                            }
                        }
                    },
                    TemperatureNoise = new ScaleBias
                    {
                        Name = "Temperature ScaleBias",
                        Scale = 0.5f,
                        Bias = 0.5f,
                        Source = new SumFractal
                        {
                            Name = "Temperature Fractal",
                            Source = new Perlin
                            {
                                Name = "Temperature Perlin",
                                Seed = 200
                            }
                        }
                    },
                    DensityNoise = new MockNoise(),
                    TerrainNoise = new MockNoise()
                };

                var componentInfoManager = new ComponentInfoManager(BiomeLoader.ComponentTypeRegistory);
                var builder = new ComponentBundleBuilder(componentInfoManager);
                builder.Add("Target", biome);
                builder.AddExternalReference(biome.DensityNoise, "title:Resources/DefaultDensityNoise.json");
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

            #region BiomeCatalogDefinition

            //================================================================
            // BiomeCatalogDefinition

            Console.WriteLine("BiomeCatalogDefinition");
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

            #region SingleBiomeManager (ComponentBundleDefinition)

            //================================================================
            // SingleBiomeManager (ComponentBundleDefinition)

            Console.WriteLine("SingleBiomeManager (ComponentBundleDefinition)");
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

            #region FlatTerrainProcedure (ComponentBundleDefinition)

            //================================================================
            // FlatTerrainProcedure (ComponentBundleDefinition)

            Console.WriteLine("FlatTerrainProcedure (ComponentBundleDefinition)");
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

            #region NoiseTerrainProcedure (ComponentBundleDefinition)

            //================================================================
            // NoiseTerrainProcedure (ComponentBundleDefinition)

            Console.WriteLine("NoiseTerrainProcedure (ComponentBundleDefinition)");
            {
                var procedure = new NoiseTerrainProcedure
                {
                    Name = "Default Noise Terrain Procedure",
                };

                var componentInfoManager = new ComponentInfoManager(ChunkProcedureLoader.ComponentTypeRegistory);
                var builder = new ComponentBundleBuilder(componentInfoManager);
                builder.Add("Target", procedure);

                ComponentBundleDefinition bundle;
                builder.BuildDefinition(out bundle);

                var jsonResource = SerializeToJson<ComponentBundleDefinition>("DefaultNoiseTerrainProcedure", bundle);
                var xmlResource = SerializeToXml<ComponentBundleDefinition>("DefaultNoiseTerrainProcedure", bundle);
                var fromJson = DeserializeFromJson<ComponentBundleDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<ComponentBundleDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region RegionDefinition

            //================================================================
            // RegionDefinition

            Console.WriteLine("RegionDefinition");
            {
                var region = new RegionDefinition
                {
                    Name = "Default Region",
                    Bounds = new BoundingBoxI
                    {
                        Min = VectorI3.Zero,
                        Max = new VectorI3(16, 16, 16)
                    },
                    TileCatalog = "DefaultTileCatalog.json",
                    BlockCatalog = "DefaultBlockCatalog.json",
                    BiomeManager = "DefaultBiomeManager.json",
                    ChunkProcedures = new string[]
                    {
                        "DefaultNoiseTerrainProcedure.json"
                    }
                };
                var jsonResource = SerializeToJson<RegionDefinition>("DefaultRegion", region);
                var xmlResource = SerializeToXml<RegionDefinition>("DefaultRegion", region);
                var fromJson = DeserializeFromJson<RegionDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<RegionDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region MeshDefinition

            //================================================================
            // MeshDefinition

            Console.WriteLine("MeshDefinition (Cube)");
            {
                var cubeMesh = CreateCubeMeshDefinition();

                var jsonResource = SerializeToJson<MeshDefinition>("Cube", cubeMesh);
                var xmlResource = SerializeToXml<MeshDefinition>("Cube", cubeMesh);
                var fromJson = DeserializeFromJson<MeshDefinition>(jsonResource);
                var fromXml = DeserializeFromXml<MeshDefinition>(xmlResource);
            }
            Console.WriteLine();

            #endregion

            #region Exit

            //================================================================
            // Exit

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            #endregion
        }

        #region Serialization/Deserialization Helpers

        static IResource SerializeToJson<T>(string baseFileName, T instance)
        {
            var serializer = new JsonSerializerAdapter(typeof(T));
            serializer.JsonSerializer.Formatting = Newtonsoft.Json.Formatting.Indented;

            var resource = FileResourceLoader.Instance.LoadResource("file:///" + directoryPath + "/" + baseFileName + ".json");
            using (var stream = resource.Create())
            {
                serializer.Serialize(stream, instance);
            }

            Console.WriteLine("Serialized: " + Path.GetFileName(resource.AbsoluteUri));
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

            Console.WriteLine("Serialized: " + Path.GetFileName(resource.AbsoluteUri));
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
            Console.WriteLine("Deserialized: " + Path.GetFileName(resource.AbsoluteUri));
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
            Console.WriteLine("Deserialized: " + Path.GetFileName(resource.AbsoluteUri));
            return result;
        }

        #endregion

        #region Utilities

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
