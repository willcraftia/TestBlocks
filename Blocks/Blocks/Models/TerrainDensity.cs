#region Using

using System;
using Willcraftia.Xna.Framework.Noise;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class TerrainDensity : INoiseSource
    {
        INoiseSource source;

        public string Name { get; set; }

        public INoiseSource Source
        {
            get { return source; }
            set { source = value; }
        }

        // I/F
        public float Sample(float x, float y, float z)
        {
            // Source が返す値がブロック空間上でのスケールに従い、
            // ハイトマップとしての値を返すことを前提としている。
            var height = source.Sample(x, y, z);

            // ハイトマップが示す高さ以下ならば密度 1 (ブロック有り)、
            // ハイトマップが示す高さより上ならば密度 0 (ブロック無し)。
            return (y <= height) ? 1 : 0;
        }

        #region ToString

        public override string ToString()
        {
            return "[Name:" + (Name ?? string.Empty) + "]";
        }

        #endregion
    }
}
