#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class InterChunkMesh
    {
        public InterChunkMeshPart Translucent { get; private set; }

        public InterChunkMeshPart Opaque { get; private set; }

        public bool Completed { get; set; }

        public InterChunkMesh()
        {
            Opaque = new InterChunkMeshPart();
            Translucent = new InterChunkMeshPart();
        }
    }
}
