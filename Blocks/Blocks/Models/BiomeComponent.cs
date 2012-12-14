#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.Noise;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class BiomeComponent : IComponentInitializable
    {
        public string Name { get; set; }

        public INoiseSource HumidityNoise { get; set; }

        public INoiseSource TemperatureNoise { get; set; }

        // I/F
        public void Initialize()
        {
            //if (HumidityNoise == null) throw new InvalidOperationException("HumidityNoise is null.");
            //if (TemperatureNoise == null) throw new InvalidOperationException("TemperatureNoise is null.");
        }

        public float GetHumidity(int x, int z)
        {
            float xf = x / (float) Biome.SizeX;
            float zf = z / (float) Biome.SizeZ;
            return MathExtension.Saturate(HumidityNoise.Sample(xf, 0, zf));
        }

        public float GetTemperature(int x, int z)
        {
            float xf = x / (float) Biome.SizeX;
            float zf = z / (float) Biome.SizeZ;
            return MathExtension.Saturate(HumidityNoise.Sample(xf, 0, zf));
        }

        public BiomeElement GetBiomeElement(int x, int z)
        {
            //
            // respect for Terasology logic.
            //

            var humidity = GetHumidity(x, z);
            var temperature = GetTemperature(x, z);

            if (0.5f <= temperature && humidity < 0.3f)
                return BiomeElement.Desert;
            if (0.5f <= temperature && 0.3f < humidity && humidity <= 0.6f)
                return BiomeElement.Plains;
            if (temperature <= 0.3f && 0.5f < humidity)
                return BiomeElement.Snow;
            if (temperature < 0.5f && 0.2f <= humidity && humidity <= 0.6f)
                return BiomeElement.Mountains;

            return BiomeElement.Forest;
        }
    }
}
