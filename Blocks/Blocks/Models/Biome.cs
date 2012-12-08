#region Using

using System;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public abstract class Biome
    {
        public VectorI3 Position;

        public int SizeX;

        public int SizeZ;

        public abstract BiomeElement this[int x, int z] { get; }

        public abstract void Initialize();
    }
}
