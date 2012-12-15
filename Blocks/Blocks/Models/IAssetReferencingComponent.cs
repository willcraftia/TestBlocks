#region Using

using System;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public interface IAssetReferencingComponent
    {
        void BindAssets(AssetManager assetManager, ResourceManager resourceManager, IResource componentResource);
    }
}
