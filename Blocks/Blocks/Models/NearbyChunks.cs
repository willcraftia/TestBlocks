#region Using

using System;
using Willcraftia.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public struct NearbyChunks
    {
        public Chunk Top;

        public Chunk Bottom;

        public Chunk Front;

        public Chunk Back;

        public Chunk Left;

        public Chunk Right;

        public Chunk this[CubeSides side]
        {
            get
            {
                switch (side)
                {
                    case CubeSides.Top: return Top;
                    case CubeSides.Bottom: return Bottom;
                    case CubeSides.Front: return Front;
                    case CubeSides.Back: return Back;
                    case CubeSides.Left: return Left;
                    case CubeSides.Right: return Right;
                    default: throw new InvalidOperationException();
                }
            }
            set
            {
                switch (side)
                {
                    case CubeSides.Top: Top = value; break;
                    case CubeSides.Bottom: Bottom = value; break;
                    case CubeSides.Front: Front = value; break;
                    case CubeSides.Back: Back = value; break;
                    case CubeSides.Left: Left = value; break;
                    case CubeSides.Right: Right = value; break;
                    default: throw new InvalidOperationException();
                }
            }
        }
    }
}
