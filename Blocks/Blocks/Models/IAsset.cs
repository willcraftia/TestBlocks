#region Using

using System;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public interface IAsset
    {
        IResource Resource { get; set; }
    }
}
