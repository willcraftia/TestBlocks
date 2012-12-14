#region Using

using System;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Noise;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class BiomeTemplate : IAsset
    {
        public const string ComponentName = "Component";

        // block unit
        public const int SizeX = 256;

        // block unit
        public const int SizeY = 256;

        // block unit
        public const int SizeZ = 256;

        static readonly AliasTypeRegistory typeRegistory = new AliasTypeRegistory();

        BiomeTemplateComponent component;

        // I/F
        public IResource Resource { get; set; }

        public int Index { get; set; }

        public ComponentFactory ComponentFactory { get; private set; }

        public BiomeTemplateComponent Component
        {
            get
            {
                if (component == null)
                    component = ComponentFactory[ComponentName] as BiomeTemplateComponent;
                return component;
            }
        }

        static BiomeTemplate()
        {
            NoiseHelper.SetTypeAliases(typeRegistory);
            typeRegistory.SetTypeAlias(typeof(BiomeTemplateComponent));
        }

        public BiomeTemplate()
        {
            ComponentFactory = new ComponentFactory(typeRegistory);
        }

        public float GetHumidity(int x, int z)
        {
            return component.GetHumidity(x, z);
        }

        public float GetTemperature(int x, int z)
        {
            return component.GetTemperature(x, z);
        }

        public BiomeElement GetBiomeElement(int x, int z)
        {
            return component.GetBiomeElement(x, z);
        }
    }
}
