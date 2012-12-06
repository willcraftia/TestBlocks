#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public struct BuilderDefinition
    {
        // Full name
        public string Type;

        public PropertyDefinition[] Properties;
    }
}
