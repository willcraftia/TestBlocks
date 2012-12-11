#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public interface INoiseSource
    {
        float Sample(float x, float y, float z);
    }
}
