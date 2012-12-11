#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Noise.Definitions
{
    public sealed class NoiseSourceTypeAlias
    {
        static readonly Dictionary<string, Type> dictionary = new Dictionary<string, Type>();

        static readonly Dictionary<Type, string> reverseDictionary = new Dictionary<Type, string>();

        static NoiseSourceTypeAlias()
        {
            Register(typeof(Perlin));
            Register(typeof(ClassicPerlin));
            Register(typeof(Simplex));
            Register(typeof(Voronoi));

            Register(typeof(Add));
            Register(typeof(Select));
            Register(typeof(ScaleBias));

            Register(typeof(Turbulence));
            Register(typeof(SumFractal));
            Register(typeof(SinFractal));
            Register(typeof(PerlinFractal));
            Register(typeof(Multifractal));
            Register(typeof(RidgedMultifractal));
            Register(typeof(HybridMultifractal));
            Register(typeof(Heterofractal));
        }

        static void Register(Type type)
        {
            dictionary[type.Name] = type;
            reverseDictionary[type] = type.Name;
        }

        public static Type GetType(string alias)
        {
            if (alias == null) throw new ArgumentNullException("alias");

            Type type;
            dictionary.TryGetValue(alias, out type);
            return type;
        }

        public static string GetAlias(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            string alias;
            reverseDictionary.TryGetValue(type, out alias);
            return alias;
        }
    }
}
