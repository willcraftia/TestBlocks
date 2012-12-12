#region Using

using System;
using Willcraftia.Xna.Framework.Component;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    [Component]
    public interface INoiseSource
    {
        float Sample(float x, float y, float z);
    }
}
