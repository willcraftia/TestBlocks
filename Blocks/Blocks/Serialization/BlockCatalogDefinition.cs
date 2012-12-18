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
    }
}
