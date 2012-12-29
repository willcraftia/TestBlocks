#region Using

using System;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkMesh
    {
        public ChunkMeshPart Opaque { get; private set; }

        public ChunkMeshPart Translucent { get; private set; }

        public ChunkMesh(Region region)
        {
            if (region == null) throw new ArgumentNullException("region");

            Translucent = new ChunkMeshPart(region, true);
            Opaque = new ChunkMeshPart(region, false);
        }
    }
}
