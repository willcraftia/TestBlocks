#region Using

using System;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Framework.Content
{
    public sealed class AssetHolder
    {
        public IResource Resource { get; set; }

        public object Asset { get; set; }
    }
}
