#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework
{
    public struct RectangleF
    {
        public float X;

        public float Y;

        public float Width;

        public float Height;

        public static RectangleF One
        {
            get { return new RectangleF { X = 0, Y = 0, Width = 1, Height = 1 }; }
        }
    }
}
