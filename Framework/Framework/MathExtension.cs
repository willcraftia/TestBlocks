#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework
{
    public static class MathExtension
    {
        public static int Floor(float value)
        {
            // Faster than using (int) Math.Floor(x).
            return 0 <= value ? (int) value : (int) value - 1;
        }

        public static float Saturate(float value)
        {
            if (1 < value) return 1;
            if (value < 0) return 0;
            return value;
        }
    }
}
