#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Content
{
    public interface IAssetManagerAware
    {
        AssetManager AssetManager { set; }
    }
}
