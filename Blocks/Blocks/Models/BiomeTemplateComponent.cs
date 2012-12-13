#region Using

using System;
using Willcraftia.Xna.Framework.Noise;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class BiomeTemplateComponent
    {
        public string Name { get; set; }

        public INoiseSource NoiseSource { get; set; }
    }
}
