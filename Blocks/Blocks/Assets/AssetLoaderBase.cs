#region Using

using System;
using Willcraftia.Xna.Framework.Assets;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public abstract class AssetLoaderBase : IAssetLoader
    {
        // I/F
        public AssetManager AssetManager { get; set; }

        protected ResourceManager ResourceManager { get; private set; }

        protected AssetLoaderBase(ResourceManager resourceManager)
        {
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");

            ResourceManager = resourceManager;
        }

        // I/F
        public abstract object Load(IResource resource);

        // I/F
        public virtual void Unload(IResource resource, object asset) { }

        // I/F
        public virtual void Save(IResource resource, object asset)
        {
            throw new NotSupportedException();
        }
    }
}
