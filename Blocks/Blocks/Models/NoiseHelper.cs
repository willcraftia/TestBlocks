#region Using

using System;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.Noise;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public static class NoiseHelper
    {
        public static void SetTypeAliases(AliasTypeRegistory typeRegistory)
        {
            if (typeRegistory == null) throw new ArgumentNullException("typeRegistory");

            // Gradient noises.
            typeRegistory.SetTypeAlias(typeof(Perlin));
            typeRegistory.SetTypeAlias(typeof(ClassicPerlin));
            typeRegistory.SetTypeAlias(typeof(Simplex));

            // Perlin fractal function.
            typeRegistory.SetTypeAlias(typeof(PerlinFractal));

            // Musgrave fractal functions.
            typeRegistory.SetTypeAlias(typeof(Heterofractal));
            typeRegistory.SetTypeAlias(typeof(HybridMultifractal));
            typeRegistory.SetTypeAlias(typeof(Multifractal));
            typeRegistory.SetTypeAlias(typeof(RidgedMultifractal));
            typeRegistory.SetTypeAlias(typeof(SinFractal));
            typeRegistory.SetTypeAlias(typeof(SumFractal));

            // Controllers.
            typeRegistory.SetTypeAlias(typeof(Add));
            typeRegistory.SetTypeAlias(typeof(ScaleBias));
            typeRegistory.SetTypeAlias(typeof(Select));
        }
    }
}
