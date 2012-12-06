#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Noise
{
    public static class NoiseArrayF2Extension
    {
        public static float Min(this INoiseArray2<float> map)
        {
            return Min(map, 0, 0, map.Width, map.Height);
        }

        public static float Min(this INoiseArray2<float> map, int startX, int startY, int sizeX, int sizeY)
        {
            float min = float.MaxValue;

            for (int y = 0; y < sizeY; y++)
                for (int x = 0; x < sizeX; x++)
                    min = Math.Min(min, map[x + startX, y + startY]);

            return min;
        }

        public static float Max(this INoiseArray2<float> map)
        {
            return Max(map, 0, 0, map.Width, map.Height);
        }

        public static float Max(this INoiseArray2<float> map, int startX, int startY, int sizeX, int sizeY)
        {
            float max = float.MinValue;

            for (int y = 0; y < sizeY; y++)
                for (int x = 0; x < sizeX; x++)
                    max = Math.Max(max, map[x + startX, y + startY]);

            return max;
        }

        public static void MinMax(this INoiseArray2<float> map, out float min, out float max)
        {
            MinMax(map, 0, 0, map.Width, map.Height, out min, out max);
        }

        public static void MinMax(this INoiseArray2<float> map, int startX, int startY, int sizeX, int sizeY, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    var v = map[x + startX, y + startY];
                    min = Math.Min(min, v);
                    max = Math.Max(max, v);
                }
            }
        }

        //public static IMap<float> Transform(this IMap<float> map, Func<float, float> operation)
        //{
        //    for (int y = 0; y < map.Height; y++)
        //        for (int x = 0; x < map.Width; x++)
        //            map[x, y] = operation(map[x, y]);

        //    return map;
        //}

        public static INoiseArray2<float> Fill(this INoiseArray2<float> map, float value)
        {
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    map[x, y] = value;

            return map;
        }

        public static INoiseArray2<float> Add(this INoiseArray2<float> map, INoiseArray2<float> other)
        {
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    map[x, y] += other[x, y];

            return map;
        }

        public static INoiseArray2<float> Subtract(this INoiseArray2<float> map, INoiseArray2<float> other)
        {
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    map[x, y] -= other[x, y];

            return map;
        }

        public static INoiseArray2<float> Multiply(this INoiseArray2<float> map, INoiseArray2<float> other)
        {
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    map[x, y] *= other[x, y];

            return map;
        }

        public static INoiseArray2<float> Divide(this INoiseArray2<float> map, INoiseArray2<float> other)
        {
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    map[x, y] /= other[x, y];

            return map;
        }

        public static INoiseArray2<float> Multiply(this INoiseArray2<float> map, float factor)
        {
            if (factor == 1)
                return map;

            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    map[x, y] *= factor;

            return map;
        }

        public static INoiseArray2<float> Normalize(this INoiseArray2<float> map)
        {
            float min;
            float max;
            MinMax(map, out min, out max);

            float length = max - min;
            if (length == 0 || length == 1)
                return map;

            float factor = 1 / length;
            return Multiply(map, factor);
        }

        public static INoiseArray2<float> NormalizeSymmetric(this INoiseArray2<float> map)
        {
            float min;
            float max;
            MinMax(map, out min, out max);

            if (1 - max < min)
                min = 1 - max;
            if (max < 1 - min)
                max = 1 - min;

            float length = max - min;
            if (length == 0)
                return map;

            float factor = 1 / length;
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    map[x, y] = (map[x, y] - min) * factor;

            return map;
        }

        public static INoiseArray2<float> Clamp(this INoiseArray2<float> map)
        {
            return Clamp(map, 0, 1);
        }

        public static INoiseArray2<float> Clamp(this INoiseArray2<float> map, float min, float max)
        {
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var v = map[x, y];
                    if (v < min)
                    {
                        map[x, y] = min;
                    }
                    else if (max < v)
                    {
                        map[x, y] = max;
                    }
                }
            }

            return map;
        }

        public static void CopyTo(this INoiseArray2<float> map, INoiseArray2<float> destination)
        {
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    destination[x, y] = map[x, y];
        }
    }
}
