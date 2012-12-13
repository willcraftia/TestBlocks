#region Using

using System;
using System.Xml.Serialization;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public struct BundleEntryDefinition
    {
        [XmlAttribute]
        public string Name;

        public ComponentDefinition Component;
    }
}
