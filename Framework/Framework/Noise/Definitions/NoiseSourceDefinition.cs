#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise.Definitions
{
    public struct NoiseSourceDefinition
    {
        public string Name;

        public string Type;

        public NoiseParameterDefinition[] Parameters;

        public NoiseReferenceDefinition[] References;
    }
}
