#region Using

using System;
using System.Xml.Serialization;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public struct ComponentDefinition
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public string Type;

        [XmlArrayItem("Property")]
        public PropertyDefinition[] Properties;
    }
}
