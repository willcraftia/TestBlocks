#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class Mesh : IAsset
    {
        // I/F
        public IResource Resource { get; set; }

        public string Name { get; set; }

        public CubicCollection<MeshPart> MeshParts { get; private set; }

        public Mesh()
        {
            MeshParts = new CubicCollection<MeshPart>();
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri:" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + "]";
        }

        #endregion
    }
}
