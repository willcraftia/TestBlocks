#region Using

using System;
using System.Xml.Serialization;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    [XmlRoot("Mesh")]
    public struct MeshDefinition
    {
        //----------------------------
        // Editor/Debug

        public string Name;

        //----------------------------
        // MeshPartDefinition

        public MeshPartDefinition Top;

        public MeshPartDefinition Bottom;
        
        public MeshPartDefinition Front;
        
        public MeshPartDefinition Back;
        
        public MeshPartDefinition Left;
        
        public MeshPartDefinition Right;
    }
}
