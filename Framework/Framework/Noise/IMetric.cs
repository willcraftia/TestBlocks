#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public interface IMetric
    {
        float Calculate(float x, float y, float z);
    }
}
