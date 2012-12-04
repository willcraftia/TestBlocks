#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public sealed class MeshDefinition
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
