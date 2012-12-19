#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Noise;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class DefaultBiome : IBiome, IInitializingComponent
    {
        #region Range

        public sealed class Range
        {
            float minTemperature;

            float maxTemperature;

            float minHumidity;

            float maxHumidity;

            public float MinTemperature
            {
                get { return minTemperature; }
                set { minTemperature = MathExtension.Saturate(value); }
            }

            public float MaxTemperature
            {
                get { return maxTemperature; }
                set { maxTemperature = MathExtension.Saturate(value); }
            }

            public float MinHumidity
            {
                get { return minHumidity; }
                set { minHumidity = MathExtension.Saturate(value); }
            }

            public float MaxHumidity
            {
                get { return maxHumidity; }
                set { maxHumidity = MathExtension.Saturate(value); }
            }

            public bool Contains(float temperature, float humidity)
            {
                // 境界を厳密に判定するのは面倒なので簡易判定。
                return minTemperature <= temperature && temperature <= maxTemperature &&
                    minHumidity <= humidity && humidity <= maxHumidity;
            }

            #region ToString

            public override string ToString()
            {
                return "[" + minTemperature + " <= temperature <= " + maxTemperature +
                    ", " + minHumidity + " <= humidity <= " + maxHumidity + "]";
            }

            #endregion
        }

        #endregion

        const float inverseSizeX = 1 / (float) BiomeBounds.SizeX;

        const float inverseSizeZ = 1 / (float) BiomeBounds.SizeZ;

        //====================================================================
        //
        // Persistent Properties
        //

        public string Name { get; set; }

        public INoiseSource TemperatureNoise { get; set; }

        public INoiseSource HumidityNoise { get; set; }

        public BiomeElement BaseElement { get; set; }

        public Range DesertRange { get; set; }

        public Range PlainsRange { get; set; }

        public Range SnowRange { get; set; }

        public Range MountainsRange { get; set; }

        public Range ForestRange { get; set; }

        //
        //====================================================================

        // I/F
        [PropertyIgnored]
        public IResource Resource { get; set; }

        [PropertyIgnored]
        public byte Index { get; set; }

        public DefaultBiome()
        {
            BaseElement = BiomeElement.Forest;
            DesertRange = new Range
            {
                MinTemperature = 0.5f,
                MaxTemperature = 1.0f,
                MinHumidity = 0.0f,
                MaxHumidity = 0.3f
            };
            PlainsRange = new Range
            {
                MinTemperature = 0.5f,
                MaxTemperature = 1.0f,
                MinHumidity = 0.3f,
                MaxHumidity = 0.6f
            };
            SnowRange = new Range
            {
                MinTemperature = 0.0f,
                MaxTemperature = 0.3f,
                MinHumidity = 0.6f,
                MaxHumidity = 1.0f
            };
            MountainsRange = new Range
            {
                MinTemperature = 0.0f,
                MaxTemperature = 0.5f,
                MinHumidity = 0.0f,
                MaxHumidity = 0.6f
            };
            ForestRange = new Range
            {
                MinTemperature = 0.3f,
                MaxTemperature = 1.0f,
                MinHumidity = 0.6f,
                MaxHumidity = 1.0f
            };
        }

        // I/F
        public void Initialize()
        {
            if (TemperatureNoise == null) throw new InvalidOperationException("TemperatureNoise is null.");
            if (HumidityNoise == null) throw new InvalidOperationException("HumidityNoise is null.");
        }

        // I/F
        public float GetTemperature(int x, int z)
        {
            float xf = x * inverseSizeX;
            float zf = z * inverseSizeZ;
            return MathExtension.Saturate(TemperatureNoise.Sample(xf, 0, zf));
        }

        // I/F
        public float GetHumidity(int x, int z)
        {
            float xf = x * inverseSizeX;
            float zf = z * inverseSizeZ;
            return MathExtension.Saturate(HumidityNoise.Sample(xf, 0, zf));
        }

        // I/F
        public BiomeElement GetBiomeElement(int x, int z)
        {
            var temperature = GetTemperature(x, z);
            var humidity = GetHumidity(x, z);

            if (DesertRange.Contains(temperature, humidity))
                return BiomeElement.Desert;

            if (PlainsRange.Contains(temperature, humidity))
                return BiomeElement.Plains;

            if (SnowRange.Contains(temperature, humidity))
                return BiomeElement.Snow;

            if (MountainsRange.Contains(temperature, humidity))
                return BiomeElement.Mountains;

            if (ForestRange.Contains(temperature, humidity))
                return BiomeElement.Forest;

            return BaseElement;
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri=" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + ", Index=" + Index + "]";
        }

        #endregion
    }
}
