﻿#region Using

using System;
using Willcraftia.Xna.Framework.Content;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public interface IBiome : IAsset
    {
        byte Index { get; set; }

        float GetTemperature(int x, int z);

        float GetHumidity(int x, int z);

        BiomeElement GetBiomeElement(int x, int z);
    }
}