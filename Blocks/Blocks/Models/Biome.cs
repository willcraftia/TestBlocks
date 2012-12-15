#region Using

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

        public IBiomeCore Core { get; set; }

        public float GetTemperature(int x, int z)
        {
            return Core.GetTemperature(x, z);
        }

        public float GetHumidity(int x, int z)
        {
            return Core.GetHumidity(x, z);
        }

        public BiomeElement GetBiomeElement(int x, int z)
        {
            return Core.GetBiomeElement(x, z);
        }
    }
}
