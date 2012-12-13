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
        public const string ComponentName = "BiomeTemplate";

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
            typeRegistory.SetTypeAlias(typeof(Perlin));
            typeRegistory.SetTypeAlias(typeof(SumFractal));
            typeRegistory.SetTypeAlias(typeof(BiomeTemplateComponent), "BiomeTemplate");
        }

        public BiomeTemplate()
        {
            ComponentFactory = new ComponentFactory(typeRegistory);
        }
    }
}
