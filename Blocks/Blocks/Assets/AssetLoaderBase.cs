#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Assets;

#endregion

namespace Willcraftia.Xna.Blocks.Assets
{
    public abstract class AssetLoaderBase : IAssetLoader
    {
        // I/F
        public AssetManager AssetManager { get; set; }

        protected UriManager UriManager { get; private set; }

        protected AssetLoaderBase(UriManager uriManager)
        {
            if (uriManager == null) throw new ArgumentNullException("uriManager");

            UriManager = uriManager;
        }

        // I/F
        public abstract object Load(IUri uri);

        // I/F
        public virtual void Unload(IUri uri, object asset) { }

        // I/F
        public virtual void Save(IUri uri, object asset)
        {
            throw new NotSupportedException();
        }
    }
}
