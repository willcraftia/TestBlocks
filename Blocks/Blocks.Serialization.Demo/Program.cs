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
        class MockBiome : IBiome
        {
            public byte Index { get; set; }

            public IResource Resource { get; set; }

            public float GetTemperature(int x, int z) { throw new NotImplementedException(); }

            public float GetHumidity(int x, int z) { throw new NotImplementedException(); }

            public BiomeElement GetBiomeElement(int x, int z) { throw new NotImplementedException(); }
        }

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
                    new IndexedUriDefinition
                    {
                        Index = 0, Uri = "DefaultTile.json"
                    }
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
            }
            Console.WriteLine();

            #endregion

            #region BlockDefinition

            //================================================================
            // BlockCatalogDefinition

            Console.WriteLine("BlockCatalogDefinition");
            {
                var blockCatalog = new BlockCatalogDefinition
                {
                    Name = "Default Block Catalog",
                    Entries = new IndexedUriDefinition[]
                    {
                        new IndexedUriDefinition
                        {
                            Index = 1, Uri = "DefaultBlock.json"
                        }
                    }
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
                    HumidityNoise = new SumFractal
                    {
                        Source = new Perlin { Seed = 0 }
                    },
                    TemperatureNoise = new SumFractal
                    {
                        Source = new Perlin { Seed = 1 }
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

            #region RegionDefinition

            //================================================================
            // RegionDefinition

            Console.WriteLine("RegionDefinition");
            {
                var region = new RegionDefinition
                {
                    Name = "Default Region",
                    Bounds = new BoundingBoxI(VectorI3.Zero, VectorI3.One),
                    TileCatalog = "DefaultTileCatalog.json",
                    BlockCatalog = "DefaultBlockCatalog.json",
                    BiomeManager = "DefaultBiomeManager.json",
                    ChunkProcedures = new string[]
                    {
                        "DefaultFlatTerrainProcedure.json"
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
            var mesh = new MeshDefinition();
            mesh.Name = "Default Cube";
            mesh.Top = new MeshPartDefinition
            {
                Vertices = new VertexPositionNormalTexture[]
                {
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-0.5f, 0.5f, 0.5f),
                        Normal=new Vector3(0, 1, 0),
                        TextureCoordinate =new Vector2(0, 1)
                    },
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(0.5f, 0.5f, 0.5f),
                        Normal = new Vector3(0, 1, 0),
                        TextureCoordinate = new Vector2(1, 1)
                    },
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(0.5f, 0.5f, -0.5f),
                        Normal = new Vector3(0, 1, 0),
                        TextureCoordinate = new Vector2(1, 0)
                    },
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-0.5f, 0.5f, -0.5f),
                        Normal = new Vector3(0, 1, 0),
                        TextureCoordinate = new Vector2(0, 0)
                    }
                },
                Indices = new ushort[] { 0, 1, 2, 0, 2, 3 }
            };
            mesh.Bottom = new MeshPartDefinition
            {
                Vertices = new VertexPositionNormalTexture[]
                {
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-0.5f, -0.5f, 0.5f),
                        Normal = new Vector3(0, -1, 0),
                        TextureCoordinate = new Vector2(1, 1)
                    },
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-0.5f, -0.5f, -0.5f),
                        Normal = new Vector3(0, -1, 0),
                        TextureCoordinate = new Vector2(1, 0)
                    },
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(0.5f, -0.5f, -0.5f),
                        Normal = new Vector3(0, -1, 0),
                        TextureCoordinate = new Vector2(0, 0)
                    },
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(0.5f, -0.5f, 0.5f),
                        Normal = new Vector3(0, -1, 0),
                        TextureCoordinate = new Vector2(0, 1)
                    }
                },
                Indices = new ushort[] { 0, 1, 2, 0, 2, 3 }
            };
            mesh.Front = new MeshPartDefinition
            {
                Vertices = new VertexPositionNormalTexture[]
                {
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-0.5f, 0.5f, 0.5f),
                        Normal = new Vector3(0, 0, 1),
                        TextureCoordinate = new Vector2(0, 0)
                    },
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-0.5f, -0.5f, 0.5f),
                        Normal = new Vector3(0, 0, 1),
                        TextureCoordinate = new Vector2(0, 1)
                    },
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(0.5f, -0.5f, 0.5f),
                        Normal = new Vector3(0, 0, 1),
                        TextureCoordinate = new Vector2(1, 1)
                    },
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(0.5f, 0.5f, 0.5f),
                        Normal = new Vector3(0, 0, 1),
                        TextureCoordinate = new Vector2(1, 0)
                    }
                },
                Indices = new ushort[] { 0, 1, 2, 0, 2, 3 }
            };
            mesh.Back = new MeshPartDefinition
            {
                Vertices = new VertexPositionNormalTexture[]
                {
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-0.5f, -0.5f, -0.5f),
                        Normal = new Vector3(0, 0, -1),
                        TextureCoordinate = new Vector2(1, 1)
                    },
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-0.5f, 0.5f, -0.5f),
                        Normal = new Vector3(0, 0, -1),
                        TextureCoordinate = new Vector2(1, 0)
                    },
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(0.5f, 0.5f, -0.5f),
                        Normal = new Vector3(0, 0, -1),
                        TextureCoordinate = new Vector2(0, 0)
                    },
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(0.5f, -0.5f, -0.5f),
                        Normal = new Vector3(0, 0, -1),
                        TextureCoordinate = new Vector2(0, 1)
                    }
                },
                Indices = new ushort[] { 0, 1, 2, 0, 2, 3 }
            };
            mesh.Left = new MeshPartDefinition
            {
                Vertices = new VertexPositionNormalTexture[]
                {
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-0.5f, -0.5f, 0.5f),
                        Normal = new Vector3(-1, 0, 0),
                        TextureCoordinate = new Vector2(1, 1)
                    },
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-0.5f, 0.5f, 0.5f),
                        Normal = new Vector3(-1, 0, 0),
                        TextureCoordinate = new Vector2(1, 0)
                    },
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-0.5f, 0.5f, -0.5f),
                        Normal = new Vector3(-1, 0, 0),
                        TextureCoordinate = new Vector2(0, 0)
                    },
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(-0.5f, -0.5f, -0.5f),
                        Normal = new Vector3(-1, 0, 0),
                        TextureCoordinate = new Vector2(0, 1)
                    }
                },
                Indices = new ushort[] { 0, 1, 2, 0, 2, 3 }
            };
            mesh.Right = new MeshPartDefinition
            {
                Vertices = new VertexPositionNormalTexture[]
                {
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(0.5f, -0.5f, -0.5f),
                        Normal = new Vector3(1, 0, 0),
                        TextureCoordinate = new Vector2(1, 1)
                    },
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(0.5f, 0.5f, -0.5f),
                        Normal = new Vector3(1, 0, 0),
                        TextureCoordinate = new Vector2(1, 0)
                    },
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(0.5f, 0.5f, 0.5f),
                        Normal = new Vector3(1, 0, 0),
                        TextureCoordinate = new Vector2(0, 0)
                    },
                    new VertexPositionNormalTexture
                    {
                        Position = new Vector3(0.5f, -0.5f, 0.5f),
                        Normal = new Vector3(1, 0, 0),
                        TextureCoordinate = new Vector2(0, 1)
                    }
                },
                Indices = new ushort[] { 0, 1, 2, 0, 2, 3 }
            };
            return mesh;
        }

        #endregion
    }
}
