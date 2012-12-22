#region Using

using System;

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

        public Chunk this[Side side]
        {
            get
            {
                switch (side)
                {
                    case Side.Top: return Top;
                    case Side.Bottom: return Bottom;
                    case Side.Front: return Front;
                    case Side.Back: return Back;
                    case Side.Left: return Left;
                    case Side.Right: return Right;
                    default: throw new InvalidOperationException();
                }
            }
            set
            {
                switch (side)
                {
                    case Side.Top: Top = value; break;
                    case Side.Bottom: Bottom = value; break;
                    case Side.Front: Front = value; break;
                    case Side.Back: Back = value; break;
                    case Side.Left: Left = value; break;
                    case Side.Right: Right = value; break;
                    default: throw new InvalidOperationException();
                }
            }
        }
    }
}
