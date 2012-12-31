#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public interface ILandscapeVolume
    {
        VectorI3 Center { get; set; }

        VectorI3[] GetPoints();

        void Contains(ref VectorI3 point, out bool result);
    }
}
