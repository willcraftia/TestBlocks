﻿#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Assets
{
    public sealed class AssetHolder
    {
        public Uri Uri { get; set; }

        public object Asset { get; set; }

        public IAssetLoader Loader { get; set; }
    }
}
