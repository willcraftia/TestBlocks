#region Using

using System;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class BiomeManager : IAsset
    {
        public const string ComponentName = "Target";

        static readonly ComponentTypeRegistory componentTypeRegistory = new ComponentTypeRegistory();

        // I/F
        public IResource Resource { get; set; }
        
        public ComponentFactory ComponentFactory { get; private set; }

        public IBiomeComponent Component { get; set; }

        static BiomeManager()
        {
            NoiseHelper.SetTypeDefinitionNames(componentTypeRegistory);

            // 利用可能な実体の型を全て登録しておく。
            //componentTypeRegistory.SetTypeDefinitionName(typeof(BiomeComponent));
        }

        public BiomeManager()
        {
            ComponentFactory = new ComponentFactory(componentTypeRegistory);
        }

        public Biome GetBiome(Chunk chunk)
        {
            throw new NotImplementedException();
        }
    }
}
