﻿#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public sealed class PerlinFractal : INoiseSource
    {
        public const float DefaultFrequency = 1;

        public const float DefaultLacunarity = 2;

        public const float DefaultPersistence = 0.5f;

        public const int DefaultOctave = 6;

        INoiseSource source;

        float frequency = DefaultFrequency;

        float lacunarity = DefaultLacunarity;

        float persistence = DefaultPersistence;

        int octaveCount = DefaultOctave;

        public string Name { get; set; }

        public INoiseSource Source
        {
            get { return source; }
            set { source = value; }
        }

        public float Frequency
        {
            get { return frequency; }
            set { frequency = value; }
        }

        public float Lacunarity
        {
            get { return lacunarity; }
            set { lacunarity = value; }
        }

        public float Persistence
        {
            get { return persistence; }
            set { persistence = value; }
        }

        public int OctaveCount
        {
            get { return octaveCount; }
            set { octaveCount = value; }
        }

        // I/F
        public float Sample(float x, float y, float z)
        {
            float value = 0;
            float amplitude = 1;

            x *= frequency;
            y *= frequency;
            z *= frequency;

            for (int i = 0; i < octaveCount; i++)
            {
                var signal = source.Sample(x, y, z);
                value += signal * amplitude;

                x *= lacunarity;
                y *= lacunarity;
                z *= lacunarity;

                amplitude *= persistence;
            }

            return value;
        }

        #region ToString

        public override string ToString()
        {
            return "[Name:" + (Name ?? string.Empty) + "]";
        }

        #endregion
    }
}
