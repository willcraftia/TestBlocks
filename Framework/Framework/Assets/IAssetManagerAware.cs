#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Assets
{
    public interface IAssetManagerAware
    {
        AssetManager AssetManager { set; }
    }
}
