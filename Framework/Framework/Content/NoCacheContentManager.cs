#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

#endregion

namespace Willcraftia.Xna.Framework.Content
{
    public sealed class NoCacheContentManager : ContentManager
    {
        public NoCacheContentManager(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public NoCacheContentManager(IServiceProvider serviceProvider, string rootDirectory)
            : base(serviceProvider, rootDirectory)
        {
        }

        public override T Load<T>(string assetName)
        {
            // not cache.
            return ReadAsset<T>(assetName, null);
        }
    }
}
