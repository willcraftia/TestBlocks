#region Using

using System;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class BiomeManager : IAsset
    {
        public const string ComponentName = "Component";

        static readonly ComponentTypeRegistory componentTypeRegistory = new ComponentTypeRegistory();

        IBiomeManagerComponent component;

        // I/F
        public IResource Resource { get; set; }
        
        public ComponentFactory ComponentFactory { get; private set; }

        public IBiomeManagerComponent Component
        {
            get
            {
                if (component == null)
                    component = ComponentFactory[ComponentName] as IBiomeManagerComponent;
                return component;
            }
        }

        static BiomeManager()
        {
            NoiseHelper.SetTypeAliases(componentTypeRegistory);

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
