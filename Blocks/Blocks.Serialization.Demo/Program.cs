#region Using

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Serialization;
using Willcraftia.Xna.Framework.Serialization.Json;
using Willcraftia.Xna.Framework.Serialization.Xml;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization.Demo
{
    class Program
    {
        static string directoryPath;

        static void Main(string[] args)
        {
            //================================================================
            // Setup

            directoryPath = Directory.GetCurrentDirectory() + "/Resources";
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            Console.WriteLine("Output: " + directoryPath);
            Console.WriteLine();

            JsonSerializerAdapter.Instance.JsonSerializer.Formatting = Newtonsoft.Json.Formatting.Indented;
            ExtensionSerializerRegistory.Instance[".json"] = JsonSerializerAdapter.Instance;
            ExtensionSerializerRegistory.Instance[".xml"] = XmlSerializerAdapter.Instance;

            Console.WriteLine("Press Enter to serialize/deserialize resources.");
            Console.ReadLine();

            //================================================================
            // TileDefinition

            Console.WriteLine("TileDefinition");
            var tile = new TileDefinition
            {
                Name = "Default Tile",
                Texture = "title:Resources/DefaultTile.png",
                Translucent = false,
                DiffuseColor = Color.White.PackedValue,
                EmissiveColor = Color.Black.PackedValue,
                SpecularColor = Color.Black.PackedValue,
                SpecularPower = 0
            };
            {
                var jsonUri = SerializeToJson<TileDefinition>("DefaultTile", tile);
                var xmlUri = SerializeToXml<TileDefinition>("DefaultTile", tile);
                var fromJson = Deserialize<TileDefinition>(jsonUri);
                var fromXml = Deserialize<TileDefinition>(xmlUri);
            }
            Console.WriteLine();

            //================================================================
            // TileCatalogDefinition

            Console.WriteLine("TileCatalogDefinition");
            var tileCatalog = new TileCatalogDefinition
            {
                Name = "Default Tile Catalog",
                Entries = new TileIndexDefinition[]
                {
                    new TileIndexDefinition
                    {
                        Index = 0,
                        Tile = "title:Resources/DefaultTile.json"
                    }
                }
            };
            {
                var jsonUri = SerializeToJson<TileCatalogDefinition>("DefaultTileCatalog", tileCatalog);
                var xmlUri = SerializeToXml<TileCatalogDefinition>("DefaultTileCatalog", tileCatalog);
                var fromJson = Deserialize<TileCatalogDefinition>(jsonUri);
                var fromXml = Deserialize<TileCatalogDefinition>(xmlUri);
            }
            Console.WriteLine();

            //================================================================
            // BlockDefinition

            Console.WriteLine("BlockDefinition");
            var block = new BlockDefinition
            {
                Name = "Default Block",
                Mesh = "title:Resources/Cube.json",
                TopTile = "title:Resources/DefaultTile.json",
                BottomTile = "title:Resources/DefaultTile.json",
                FrontTile = "title:Resources/DefaultTile.json",
                BackTile = "title:Resources/DefaultTile.json",
                LeftTile = "title:Resources/DefaultTile.json",
                RightTile = "title:Resources/DefaultTile.json",
                Fluid = false,
                ShadowCasting = true,
                Shape = BlockShape.Cube,
                Mass = 1,
                StaticFriction = 0.5f,
                DynamicFriction = 0.5f,
                Restitution = 0.5f
            };
            {
                var jsonUri = SerializeToJson<BlockDefinition>("DefaultBlock", block);
                var xmlUri = SerializeToXml<BlockDefinition>("DefaultBlock", block);
                var fromJson = Deserialize<BlockDefinition>(jsonUri);
                var fromXml = Deserialize<BlockDefinition>(xmlUri);
            }
            Console.WriteLine();

            //================================================================
            // BlockCatalogDefinition

            Console.WriteLine("BlockCatalogDefinition");
            var blockCatalog = new BlockCatalogDefinition
            {
                Name = "Default Block",
                Entries = new BlockIndexDefinition[]
                {
                    new BlockIndexDefinition
                    {
                        Index = 1,
                        Block = "title:Resources/DefaultBlock.json"
                    }
                }
            };
            {
                var jsonUri = SerializeToJson<BlockCatalogDefinition>("DefaultBlockCatalog", blockCatalog);
                var xmlUri = SerializeToXml<BlockCatalogDefinition>("DefaultBlockCatalog", blockCatalog);
                var fromJson = Deserialize<BlockCatalogDefinition>(jsonUri);
                var fromXml = Deserialize<BlockCatalogDefinition>(xmlUri);
            }
            Console.WriteLine();

            //================================================================
            // RegionDefinition

            Console.WriteLine("RegionDefinition");
            var region = new RegionDefinition
            {
                Name = "Default Region",
                Bounds = new BoundingBoxI(VectorI3.Zero, VectorI3.One),
                TileCatalog = "title:Resources/DefaultTileCatalog.json",
                BlockCatalog = "title:Resources/DefaultBlockCatalog.json",
                ChunkProcedures = new ProcedureDefinition[]
                {
                    new ProcedureDefinition
                    {
                        Type = "Willcraftia.Xna.Blocks.Models.FlatTerrainBuilder",
                        Properties = new PropertyDefinition[]
                        {
                            new PropertyDefinition
                            {
                                Name = "Property1",
                                Value = "1"
                            }
                        }
                    }
                }
            };
            {
                var jsonUri = SerializeToJson<RegionDefinition>("DefaultRegion", region);
                var xmlUri = SerializeToXml<RegionDefinition>("DefaultRegion", region);
                var fromJson = Deserialize<RegionDefinition>(jsonUri);
                var fromXml = Deserialize<RegionDefinition>(xmlUri);
            }
            Console.WriteLine();

            //================================================================
            // MeshDefinition

            Console.WriteLine("MeshDefinition (Cube)");
            var cubeMesh = CreateCubeMeshDefinition();
            {
                var jsonUri = SerializeToJson<MeshDefinition>("Cube", cubeMesh);
                var xmlUri = SerializeToXml<MeshDefinition>("Cube", cubeMesh);
                var fromJson = Deserialize<MeshDefinition>(jsonUri);
                var fromXml = Deserialize<MeshDefinition>(xmlUri);
            }
            Console.WriteLine();

            //================================================================
            // Exit

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }

        static Uri SerializeToJson<T>(string baseFileName, T resource)
        {
            var uri = new Uri(directoryPath + "/" + baseFileName + ".json");
            ResourceSerializer.Serialize<T>(uri, resource);
            Console.WriteLine("Serialized: " + Path.GetFileName(uri.LocalPath));
            return uri;
        }

        static Uri SerializeToXml<T>(string baseFileName, T resource)
        {
            var uri = new Uri(directoryPath + "/" + baseFileName + ".xml");
            ResourceSerializer.Serialize<T>(uri, resource);
            Console.WriteLine("Serialized: " + Path.GetFileName(uri.LocalPath));
            return uri;
        }

        static T Deserialize<T>(Uri uri)
        {
            var result = ResourceSerializer.Deserialize<T>(uri);
            Console.WriteLine("Deserialized: " + Path.GetFileName(uri.LocalPath));
            return result;
        }

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
    }
}
