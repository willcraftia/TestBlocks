#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public struct TileDefinition
    {
        //----------------------------
        // Editor/Debug

        public string Name;

        //----------------------------
        // Texture

        public string Texture;

        public bool Translucent;

        //----------------------------
        // Lighting

        public uint DiffuseColor;

        public uint EmissiveColor;

        public uint SpecularColor;

        public byte SpecularPower;
    }
}
