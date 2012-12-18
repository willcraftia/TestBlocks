#region Using

using System;
using System.Xml.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    [XmlRoot("BiomeCatalog")]
    public struct BiomeCatalogDefinition
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
