#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class SCurve3 : IFadeCurve
    {
        public static float Function(float x)
        {
            return x * x * (3 - 2 * x);
        }

        // I/F
        public float Calculate(float x)
        {
            return Function(x);
        }
    }
}
