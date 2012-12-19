#region Using

using System;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class BiomeCatalog : KeyedList<byte, IBiome>, IAsset
    {
        // I/F
        public IResource Resource { get; set; }

        public string Name { get; set; }

        public BiomeCatalog(int capacity)
            : base(capacity)
        {
        }

        protected override byte GetKeyForItem(IBiome item)
        {
            return item.Index;
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri:" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + "]";
        }

        #endregion
    }
}
