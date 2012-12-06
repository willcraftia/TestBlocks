#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public static class FadeCurves
    {
        public static float PassThrough(float x)
        {
            return x;
        }

        public static float SCurve3(float x)
        {
            return x * x * (3 - 2 * x);
        }

        public static float SCurve5(float x)
        {
            return x * x * x * (x * (x * 6 - 15) + 10);
        }
    }
}
