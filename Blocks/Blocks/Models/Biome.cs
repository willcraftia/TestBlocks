﻿#region Using

using System;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Noise;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class Biome : IAsset
    {
        // I/F
        public IResource Resource { get; set; }

        public byte Index { get; set; }

        public ComponentFactory ComponentFactory { get; set; }

        public IBiomeComponent Component { get; set; }

        public float GetTemperature(int x, int z)
        {
            return Component.GetTemperature(x, z);
        }

        public float GetHumidity(int x, int z)
        {
            return Component.GetHumidity(x, z);
        }

        public BiomeElement GetBiomeElement(int x, int z)
        {
            return Component.GetBiomeElement(x, z);
        }
    }
}
