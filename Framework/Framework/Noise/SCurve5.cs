#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class SCurve5 : IFadeCurve
    {
        public static float Function(float x)
        {
            return x * x * x * (x * (x * 6 - 15) + 10);
        }

        // I/F
        public float Calculate(float x)
        {
            return Function(x);
        }
    }
}
