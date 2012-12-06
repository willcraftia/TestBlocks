#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public interface INoiseArray2<T>
    {
        int Width { get; }

        int Height { get; }

        T this[int x, int y] { get; set; }
    }
}
