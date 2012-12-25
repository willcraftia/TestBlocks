﻿#region Using

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

            public IResource Resource { get; set; }

            public float GetTemperature(int x, int z) { throw new NotImplementedException(); }

            public float GetHumidity(int x, int z) { throw new NotImplementedException(); }

            public BiomeElement GetBiomeElement(int x, int z) { throw new NotImplementedException(); }
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
                        Scale = 0.5f,
                        Bias = 0.5f,
                        Source = new SumFractal
                        {
                            Source = new ClassicPerlin { Seed = 300 }
                        }
                    },
                    TemperatureNoise = new ScaleBias
                    {
                        Scale = 0.5f,
                        Bias = 0.5f,
                        Source = new SumFractal
                        {
                            Source = new ClassicPerlin { Seed = 301 }
                        }
                    }
                };

                var componentInfoManager = new ComponentInfoManager(BiomeLoader.ComponentTypeRegistory);
                var builder = new ComponentBundleBuilder(componentInfoManager);
                builder.Add("Target", biome);

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

            #region FlatTerrainProcedure (ComponentBundleDefinition)

            //================================================================
            // NoiseTerrainProcedure (ComponentBundleDefinition)

            // see: http://accidentalnoise.sourceforge.net/minecraftworlds.html

            Console.WriteLine("NoiseTerrainProcedure (ComponentBundleDefinition)");
            {
                // const zero
                var constZero = new Const { Value = 0 };

                // ground_gradient
                var groundGradient = new Gradient { X1 = 0, X2 = 0, Y1 = 0, Y2 = 1 };

                // lowland_shape_fractal (no use lowland_autocorrect)
                var lowlandShapeFractal = new Billow
                {
                    OctaveCount = 2,
                    Frequency = 0.25f,
                    Source = new ClassicPerlin { Seed = 300 }
                };
                // lowland_scale
                var lowlandScale = new ScaleBias
                {
                    Scale = 0.125f,
                    Bias = -0.45f,
                    Source = lowlandShapeFractal
                };
                // lowland_y_scale
                var lowlandYScale = new ScalePoint
                {
                    ScaleY = 0,
                    Source = lowlandScale
                };
                // lowland_terrain
                var lowlandTerrain = new Displace
                {
                    DisplaceX = constZero,
                    DisplaceY = lowlandYScale,
                    DisplaceZ = constZero,
                    Source = groundGradient
                };

                // highland_shape_fractal (no use highland_autocorrect)
                var highlandShapeFractal = new SumFractal
                {
                    OctaveCount = 4,
                    Frequency = 2,
                    Source = new ClassicPerlin { Seed = 300 }
                };
                // highland_scale
                var highlandScale = new ScaleBias
                {
                    Scale = 0.25f,
                    Bias = 0,
                    Source = highlandShapeFractal
                };
                // highland_y_scale
                var highlandYScale = new ScalePoint
                {
                    ScaleY = 0,
                    Source = highlandScale
                };
                // highland_terrain
                var highlandTerrain = new Displace
                {
                    DisplaceX = constZero,
                    DisplaceY = highlandYScale,
                    DisplaceZ = constZero,
                    Source = groundGradient
                };

                // mountain_shape_fractal (no use mountain_autocorrect)
                var mountainShapeFractal = new RidgedMultifractal
                {
                    OctaveCount = 8,
                    Frequency = 1,
                    Source = new ClassicPerlin { Seed = 300 }
                };
                // mountain_scale
                var mountainScale = new ScaleBias
                {
                    Scale = 0.45f,
                    Bias = 0.15f,
                    Source = mountainShapeFractal
                };
                // mountain_y_scale
                var mountainYScale = new ScalePoint
                {
                    ScaleY = 0.25f,
                    Source = mountainScale
                };
                // mountain_terrain
                var mountainTerrain = new Displace
                {
                    DisplaceX = constZero,
                    DisplaceY = mountainYScale,
                    DisplaceZ = constZero,
                    Source = groundGradient
                };

                // terrain_type_fractal (no use terrain_autocorrect)
                var terrainTypeFractal = new SumFractal
                {
                    OctaveCount = 3,
                    Frequency = 0.125f,
                    Source = new ClassicPerlin { Seed = 300 }
                };
                // terrain_type_y_scale
                var terrainTypeYScale = new ScalePoint
                {
                    ScaleY = 0,
                    Source = terrainTypeFractal
                };
                // terrain_type_cache
                var terrainTypeCache = new Cache { Source = terrainTypeYScale };
                // highland_mountain_select
                var highlandMountainSelect = new Select
                {
                    LowerBound = 0.55f - 0.2f,
                    UpperBound = 0.55f + 0.2f,
                    LowerSource = highlandTerrain,
                    UpperSource = mountainTerrain,
                    Controller = terrainTypeCache
                };
                // highland_lowland_select
                var hightlandLowlandSelect = new Select
                {
                    LowerBound = 0.25f - 0.15f,
                    UpperBound = 0.25f + 0.15f,
                    LowerSource = lowlandTerrain,
                    UpperSource = highlandMountainSelect,
                    Controller = terrainTypeCache
                };
                // highland_lowland_select_cache
                var hightlandLowlandSelectCache = new Cache { Source = hightlandLowlandSelect };

                // ground_select
                var groundSelect = new Select
                {
                    LowerBound = 0.5f,
                    UpperBound = 10,
                    LowerSource = new Const { Value = 0 },
                    UpperSource = new Const { Value = 1 },
                    Controller = hightlandLowlandSelectCache
                };

                var value = groundSelect.Sample(0.1f, 0.1f, 0.1f);

                var procedure = new NoiseTerrainProcedure
                {
                    Name = "Default Noise Terrain Procedure",
                    Noise = groundSelect
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
