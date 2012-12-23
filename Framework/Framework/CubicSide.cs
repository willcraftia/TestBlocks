#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Willcraftia.Xna.Framework
{
    public sealed class CubicSide
    {
        [Flags]
        public enum Flags
        {
            None = 0x0,
            Top = 0x1,
            Bottom = 0x2,
            Front = 0x4,
            Back = 0x8,
            Left = 0x10,
            Right = 0x20
        }

        public const int Count = 6;

        public const int TopIndex = 0;

        public const int BottomIndex = 1;

        public const int FrontIndex = 2;

        public const int BackIndex = 3;

        public const int LeftIndex = 4;

        public const int RightIndex = 5;

        public static readonly CubicSide Top = new CubicSide(TopIndex, "Top", VectorI3.Top);

        public static readonly CubicSide Bottom = new CubicSide(BottomIndex, "Bottom", VectorI3.Bottom);

        public static readonly CubicSide Front = new CubicSide(FrontIndex, "Front", VectorI3.Front);

        public static readonly CubicSide Back = new CubicSide(BackIndex, "Back", VectorI3.Back);

        public static readonly CubicSide Left = new CubicSide(LeftIndex, "Left", VectorI3.Left);

        public static readonly CubicSide Right = new CubicSide(RightIndex, "Right", VectorI3.Right);

        public static ReadOnlyCollection<CubicSide> Items { get; private set; }

        public int Index { get; private set; }

        public string Name { get; private set; }

        public VectorI3 Direction { get; private set; }

        static CubicSide()
        {
            var list = new List<CubicSide>(Count);
            list.Add(Top);
            list.Add(Bottom);
            list.Add(Front);
            list.Add(Back);
            list.Add(Left);
            list.Add(Right);
            Items = list.AsReadOnly();
        }

        CubicSide(int index, string name, VectorI3 direction)
        {
            Index = index;
            Name = name;
            Direction = direction;
        }

        public static CubicSide ToCubicSide(int index)
        {
            switch (index)
            {
                case TopIndex: return CubicSide.Top;
                case BottomIndex: return CubicSide.Bottom;
                case FrontIndex: return CubicSide.Front;
                case BackIndex: return CubicSide.Back;
                case LeftIndex: return CubicSide.Left;
                case RightIndex: return CubicSide.Right;
            }

            throw new ArgumentOutOfRangeException("index");
        }

        public CubicSide Reverse()
        {
            switch (Index)
            {
                case TopIndex: return CubicSide.Bottom;
                case BottomIndex: return CubicSide.Top;
                case FrontIndex: return CubicSide.Back;
                case BackIndex: return CubicSide.Front;
                case LeftIndex: return CubicSide.Right;
                case RightIndex: return CubicSide.Left;
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
