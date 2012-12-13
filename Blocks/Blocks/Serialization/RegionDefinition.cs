#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Component;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public struct RegionDefinition
    {
        //----------------------------
        // Editor/Debug

        public string Name;

        //----------------------------
        // Bounds (the unit is chunk)

        public BoundingBoxI Bounds;

        //----------------------------
        // Catalog

        // URI
        public string TileCatalog;

        // URI
        public string BlockCatalog;

        //----------------------------
        // Chunk

        // URI
        public string ChunkBundle;

        public ComponentDefinition[] ChunkProcedures;
    }
}
