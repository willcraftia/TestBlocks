#region Using

using System;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Framework.Assets
{
    public interface IAssetLoader
    {
        object Load(IResource resource);

        void Save(IResource resource, object asset);
    }
}
