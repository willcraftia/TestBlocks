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
        public const string ComponentName = "Component";

        static readonly ComponentTypeRegistory componentTypeRegistory = new ComponentTypeRegistory();

        // I/F
        public IResource Resource { get; set; }

        public byte Index { get; set; }

        public ComponentFactory ComponentFactory { get; private set; }

        public IBiomeComponent Component { get; set; }

        static Biome()
        {
            NoiseHelper.SetTypeDefinitionNames(componentTypeRegistory);

            // 利用可能な実体の型を全て登録しておく。
            componentTypeRegistory.SetTypeDefinitionName(typeof(BiomeComponent));
            componentTypeRegistory.SetTypeDefinitionName(typeof(BiomeComponent.Range));
        }

        public Biome()
        {
            ComponentFactory = new ComponentFactory(componentTypeRegistory);
        }

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
