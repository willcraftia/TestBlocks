#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public interface IBiomeCore
    {
        float GetTemperature(int x, int z);

        float GetHumidity(int x, int z);

        BiomeElement GetBiomeElement(int x, int z);
    }
}
