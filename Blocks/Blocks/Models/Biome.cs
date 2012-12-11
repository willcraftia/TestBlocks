#region Using

using System;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public abstract class Biome
    {
        public const int SizeX = 256;
        
        public const int SizeY = 256;
        
        public const int SizeZ = 256;

        public VectorI3 Position;

        public abstract BiomeElement this[int x, int z] { get; }

        public abstract void Initialize();
    }
}
