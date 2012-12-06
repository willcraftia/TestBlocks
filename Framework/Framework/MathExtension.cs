#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework
{
    public static class MathExtension
    {
        public static int Floor(float v)
        {
            // Faster than using (int) Math.Floor(x).
            return 0 < v ? (int) v : (int) v - 1;
        }
    }
}
