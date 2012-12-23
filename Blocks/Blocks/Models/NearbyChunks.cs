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

        public Chunk this[CubicSide side]
        {
            get
            {
                switch (side.Index)
                {
                    case CubicSide.TopIndex: return Top;
                    case CubicSide.BottomIndex: return Bottom;
                    case CubicSide.FrontIndex: return Front;
                    case CubicSide.BackIndex: return Back;
                    case CubicSide.LeftIndex: return Left;
                    case CubicSide.RightIndex: return Right;
                }
                throw new InvalidOperationException();
            }
            set
            {
                switch (side.Index)
                {
                    case CubicSide.TopIndex: Top = value; return;
                    case CubicSide.BottomIndex: Bottom = value; return;
                    case CubicSide.FrontIndex: Front = value; return;
                    case CubicSide.BackIndex: Back = value; return;
                    case CubicSide.LeftIndex: Left = value; return;
                    case CubicSide.RightIndex: Right = value; return;
                }
                throw new InvalidOperationException();
            }
        }
    }
}
