#region Using

using System;
using System.Xml.Serialization;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Component;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    [XmlRoot("Region")]
    public struct RegionDefinition
    {
        //----------------------------
        // Editor/Debug

        public string Name;

        //----------------------------
        // Bounds (the unit is chunk)

        public IntBoundingBox Box;

        //----------------------------
        // Catalog

        // URI
        public string TileCatalog;

        // URI
        public string BlockCatalog;

        //----------------------------
        // Biome

        public string BiomeManager;

        //----------------------------
        // Chunk

        // URI
        public string ChunkBundle;

        // URI
        [XmlArrayItem("Procedure")]
        public string[] ChunkProcedures;
    }
}
