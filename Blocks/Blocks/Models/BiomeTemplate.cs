#region Using

using System;
using Willcraftia.Xna.Framework.Component;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Noise;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class BiomeTemplate : IAsset, IComponentBundleFactoryAware, IComponentNameAware
    {
        ComponentBundleFactory factory;

        string componentName;

        // Inject
        public INoiseSource NoiseSource { get; set; }

        // I/F
        [PropertyIgnored]
        public IResource Resource { get; set; }

        [PropertyIgnored]
        public int Index { get; set; }

        public string Name { get; set; }

        // I/F
        public void SetComponentBundleFactory(ComponentBundleFactory factory)
        {
            this.factory = factory;
        }

        // I/F
        public void SetComponentName(string componentName)
        {
            this.componentName = componentName;
        }
    }
}
