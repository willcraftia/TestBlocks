#region Using

using System;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Framework.Content
{
    public interface IAsset
    {
        IResource Resource { get; set; }
    }
}
