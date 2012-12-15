#region Using

using System;
using System.Xml.Serialization;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    [XmlRoot("Bundle")]
    public struct ComponentBundleDefinition
    {
        [XmlArrayItem("Component")]
        public ComponentDefinition[] Components;
    }
}
