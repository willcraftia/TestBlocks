#region Using

using System;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class BlockCatalog : KeyedList<byte, Block>, IAsset
    {
        // I/F
        public IResource Resource { get; set; }

        public string Name { get; set; }

        public byte DirtIndex { get; set; }

        public byte GrassIndex { get; set; }

        public byte MantleIndex { get; set; }

        public byte SandIndex { get; set; }

        public byte SnowIndex { get; set; }

        public byte StoneIndex { get; set; }

        public Block Dirt { get { return this[DirtIndex]; } }

        public Block Grass { get { return this[GrassIndex]; } }

        public Block Mantle { get { return this[MantleIndex]; } }

        public Block Sand { get { return this[SandIndex]; } }

        public Block Snow { get { return this[SnowIndex]; } }

        public Block Stone { get { return this[StoneIndex]; } }

        public BlockCatalog(int capacity)
            : base(capacity)
        {
        }

        protected override byte GetKeyForItem(Block item)
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
