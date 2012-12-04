#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public sealed class TileCatalogDefinition
    {
        //----------------------------
        // Editor/Debug

        public string Name;

        //----------------------------
        // Entries

        public TileIndexDefinition[] Entries;
    }
}
