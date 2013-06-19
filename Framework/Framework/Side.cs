#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Willcraftia.Xna.Framework
{
    public sealed class Side
    {
        [Flags]
        public enum Flags
        {
            None    = 0,
            Top     = (1 << 0),
            Bottom  = (1 << 1),
            Front   = (1 << 2),
            Back    = (1 << 3),
            Left    = (1 << 4),
            Right   = (1 << 5),
            All     = Top | Bottom | Front | Back | Left | Right
        }

        public const int Count = 6;

        public const int TopIndex = 0;

        public const int BottomIndex = 1;

        public const int FrontIndex = 2;

        public const int BackIndex = 3;

        public const int LeftIndex = 4;

        public const int RightIndex = 5;

        public static readonly Side Top = new Side(TopIndex, "Top", IntVector3.Top);

        public static readonly Side Bottom = new Side(BottomIndex, "Bottom", IntVector3.Bottom);

        public static readonly Side Front = new Side(FrontIndex, "Front", IntVector3.Front);

        public static readonly Side Back = new Side(BackIndex, "Back", IntVector3.Back);

        public static readonly Side Left = new Side(LeftIndex, "Left", IntVector3.Left);

        public static readonly Side Right = new Side(RightIndex, "Right", IntVector3.Right);

        public static ReadOnlyCollection<Side> Items { get; private set; }

        public int Index { get; private set; }

        public string Name { get; private set; }

        public IntVector3 Direction { get; private set; }

        static Side()
        {
            var list = new List<Side>(Count);
            list.Add(Top);
            list.Add(Bottom);
            list.Add(Front);
            list.Add(Back);
            list.Add(Left);
            list.Add(Right);
            Items = list.AsReadOnly();
        }

        Side(int index, string name, IntVector3 direction)
        {
            Index = index;
            Name = name;
            Direction = direction;
        }

        public static Side ToCubicSide(int index)
        {
            switch (index)
            {
                case TopIndex: return Side.Top;
                case BottomIndex: return Side.Bottom;
                case FrontIndex: return Side.Front;
                case BackIndex: return Side.Back;
                case LeftIndex: return Side.Left;
                case RightIndex: return Side.Right;
            }

            throw new ArgumentOutOfRangeException("index");
        }

        public Side Reverse()
        {
            switch (Index)
            {
                case TopIndex: return Side.Bottom;
                case BottomIndex: return Side.Top;
                case FrontIndex: return Side.Back;
                case BackIndex: return Side.Front;
                case LeftIndex: return Side.Right;
                case RightIndex: return Side.Left;
            }

            throw new InvalidOperationException();
        }

        public Flags ToFlags()
        {
            switch (Index)
            {
                case TopIndex: return Flags.Top;
                case BottomIndex: return Flags.Bottom;
                case FrontIndex: return Flags.Front;
                case BackIndex: return Flags.Back;
                case LeftIndex: return Flags.Left;
                case RightIndex: return Flags.Right;
            }

            throw new InvalidOperationException();
        }

        public override int GetHashCode()
        {
            return Index;
        }

        #region ToString

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
