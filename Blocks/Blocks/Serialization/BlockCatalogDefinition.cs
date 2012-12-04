#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public sealed class BlockCatalogDefinition
    {
        //----------------------------
        // Editor/Debug

        public string Name;

        //----------------------------
        // Entries

        public BlockIndexDefinition[] Entries;
    }
}
