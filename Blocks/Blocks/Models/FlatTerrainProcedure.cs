#region Using

using System;
using Willcraftia.Xna.Framework.Component;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class FlatTerrainProcedure : IProcedure<Chunk>
    {
        ComponentBundleFactory factory;

        string componentName;

        Region region;

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

        // I/F
        public ComponentBundleFactory GetComponentBundleFactory()
        {
            return factory;
        }

        // I/F
        public string GetComponentName()
        {
            return componentName;
        }

        // I/F
        public void SetRegion(Region region)
        {
            this.region = region;
        }

        // I/F
        public void Generate(Chunk instance)
        {
        }
    }
}
