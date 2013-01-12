#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework
{
    public sealed class VectorI3LengthComparer : IComparer<VectorI3>
    {
        public static VectorI3LengthComparer Instace = new VectorI3LengthComparer();

        VectorI3LengthComparer() { }

        public int Compare(VectorI3 x, VectorI3 y)
        {
            var d1 = x.LengthSquared();
            var d2 = y.LengthSquared();

            if (d1 == d2) return 0;

            return d1 < d2 ? -1 : 1;
        }
    }
}
