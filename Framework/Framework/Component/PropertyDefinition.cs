#region Using

using System;
using System.Xml.Serialization;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public struct PropertyDefinition
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public string Value;
    }
}
