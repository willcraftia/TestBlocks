#region Using

using System;
using System.Xml.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    [XmlRoot("BlockCatalog")]
    public struct BlockCatalogDefinition
    {
        //----------------------------
        // Editor/Debug

        public string Name;

        //----------------------------
        // Entries

        [XmlArrayItem("Entry")]
        public IndexedUriDefinition[] Entries;

        //----------------------------
        // Block indices for procedural terrains

        public byte Dirt;

        public byte Grass;

        public byte Mantle;

        public byte Sand;

        public byte Snow;

        public byte Stone;
    }
}
