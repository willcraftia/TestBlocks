#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class NoFadeCurve : IFadeCurve
    {
        // I/F
        public float Calculate(float x)
        {
            return x;
        }
    }
}
