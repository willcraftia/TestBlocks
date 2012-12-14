#region Using

using System;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public abstract class BiomeBounds
    {
        // block unit
        public const int SizeX = 256;

        // block unit
        public const int SizeY = 256;

        // block unit
        public const int SizeZ = 256;

        public VectorI3 Position;
    }
}
