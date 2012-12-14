#region Using

using System;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public abstract class BiomeBounds
    {
        public const int SizeX = 256;
        
        public const int SizeY = 256;
        
        public const int SizeZ = 256;

        public VectorI3 Position;
    }
}
