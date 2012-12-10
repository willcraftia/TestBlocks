#region Using

using System;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Framework.Assets
{
    public interface IResourceManagerAware
    {
        ResourceManager ResourceManager { set; }
    }
}
