#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public struct ComponentDefinition
    {
        public string Name;

        public string Type;

        public ComponentPropertyDefinition[] Properties;

        public ComponentPropertyDefinition[] References;
    }
}
