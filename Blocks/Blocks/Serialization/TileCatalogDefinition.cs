#region Using

using System;
using System.Xml.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    [XmlRoot("TileCatalog")]
    public struct TileCatalogDefinition
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
