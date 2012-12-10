#region Using

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Serialization;
using Willcraftia.Xna.Framework.Serialization.Json;

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

            Console.WriteLine("Press Enter to serialize/deserialize resources.");
            Console.ReadLine();

            //================================================================
            // TileDefinition

            Console.WriteLine("TileDefinition");
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
            {
                var jsonUri = SerializeToJson<TileDefinition>("DefaultTile", tile);
                var xmlUri = SerializeToXml<TileDefinition>("DefaultTile", tile);
                var fromJson = DeserializeFromJson<TileDefinition>(jsonUri);
                var fromXml = DeserializeFromXml<TileDefinition>(xmlUri);
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
                        Tile = "DefaultTile.json"
                    }
                }
            };
            {
                var jsonUri = SerializeToJson<TileCatalogDefinition>("DefaultTileCatalog", tileCatalog);
                var xmlUri = SerializeToXml<TileCatalogDefinition>("DefaultTileCatalog", tileCatalog);
                var fromJson = DeserializeFromJson<TileCatalogDefinition>(jsonUri);
                var fromXml = DeserializeFromXml<TileCatalogDefinition>(xmlUri);
            }
            Console.WriteLine();

            //================================================================
            // BlockDefinition

            Console.WriteLine("BlockDefinition");
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
            {
                var jsonUri = SerializeToJson<BlockDefinition>("DefaultBlock", block);
                var xmlUri = SerializeToXml<BlockDefinition>("DefaultBlock", block);
                var fromJson = DeserializeFromJson<BlockDefinition>(jsonUri);
                var fromXml = DeserializeFromXml<BlockDefinition>(xmlUri);
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
                        Block = "DefaultBlock.json"
                    }
                }
            };
            {
                var jsonUri = SerializeToJson<BlockCatalogDefinition>("DefaultBlockCatalog", blockCatalog);
                var xmlUri = SerializeToXml<BlockCatalogDefinition>("DefaultBlockCatalog", blockCatalog);
                var fromJson = DeserializeFromJson<BlockCatalogDefinition>(jsonUri);
                var fromXml = DeserializeFromXml<BlockCatalogDefinition>(xmlUri);
            }
            Console.WriteLine();

            //================================================================
            // RegionDefinition

            Console.WriteLine("RegionDefinition");
            var region = new RegionDefinition
            {
                Name = "Default Region",
                Bounds = new BoundingBoxI(VectorI3.Zero, VectorI3.One),
                TileCatalog = "DefaultTileCatalog.json",
                BlockCatalog = "DefaultBlockCatalog.json",
                //ChunkProcedures = new ProcedureDefinition[]
                //{
                //    new ProcedureDefinition
                //    {
                //        Type = "Willcraftia.Xna.Blocks.Models.FlatTerrainBuilder",
                //        Properties = new PropertyDefinition[]
                //        {
                //            new PropertyDefinition
                //            {
                //                Name = "Property1",
                //                Value = "1"
                //            }
                //        }
                //    }
                //}
            };
            {
                var jsonUri = SerializeToJson<RegionDefinition>("DefaultRegion", region);
                var xmlUri = SerializeToXml<RegionDefinition>("DefaultRegion", region);
                var fromJson = DeserializeFromJson<RegionDefinition>(jsonUri);
                var fromXml = DeserializeFromXml<RegionDefinition>(xmlUri);
            }
            Console.WriteLine();

            //================================================================
            // MeshDefinition

            Console.WriteLine("MeshDefinition (Cube)");
            var cubeMesh = CreateCubeMeshDefinition();
            {
                var jsonUri = SerializeToJson<MeshDefinition>("Cube", cubeMesh);
                var xmlUri = SerializeToXml<MeshDefinition>("Cube", cubeMesh);
                var fromJson = DeserializeFromJson<MeshDefinition>(jsonUri);
                var fromXml = DeserializeFromXml<MeshDefinition>(xmlUri);
            }
            Console.WriteLine();

            //================================================================
            // Exit

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }

        static IUri SerializeToJson<T>(string baseFileName, T resource)
        {
            var serializer = new JsonSerializerAdapter(typeof(T));
            serializer.JsonSerializer.Formatting = Newtonsoft.Json.Formatting.Indented;

            var uriString = "file:///" + directoryPath + "/" + baseFileName + ".json";
            var uri = FileUriParser.Instance.Parse(uriString);
            using (var stream = uri.Create())
            {
                serializer.Serialize(stream, resource);
            }
            Console.WriteLine("Serialized: " + Path.GetFileName(uri.AbsoluteUri));
            return uri;
        }

        static IUri SerializeToXml<T>(string baseFileName, T resource)
        {
            var serializer = new XmlSerializerAdapter(typeof(T));
            serializer.WriterSettings.Indent = true;
            serializer.WriterSettings.IndentChars = "\t";

            var uriString = "file:///" + directoryPath + "/" + baseFileName + ".xml";
            var uri = FileUriParser.Instance.Parse(uriString);
            using (var stream = uri.Create())
            {
                serializer.Serialize(stream, resource);
            }
            Console.WriteLine("Serialized: " + Path.GetFileName(uri.AbsoluteUri));
            return uri;
        }

        static T DeserializeFromJson<T>(IUri uri)
        {
            var serializer = new JsonSerializerAdapter(typeof(T));
            serializer.JsonSerializer.Formatting = Newtonsoft.Json.Formatting.Indented;

            T result;
            using (var stream = uri.Open())
            {
                result = (T) serializer.Deserialize(stream, null);
            }
            Console.WriteLine("Deserialized: " + Path.GetFileName(uri.AbsoluteUri));
            return result;
        }

        static T DeserializeFromXml<T>(IUri uri)
        {
            var serializer = new XmlSerializerAdapter(typeof(T));

            T result;
            using (var stream = uri.Open())
            {
                result = (T) serializer.Deserialize(stream, null);
            }
            Console.WriteLine("Deserialized: " + Path.GetFileName(uri.AbsoluteUri));
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
