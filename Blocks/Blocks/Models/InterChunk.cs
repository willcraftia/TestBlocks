#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class InterChunk
    {
        public InterChunkMesh Translucent { get; private set; }

        public InterChunkMesh Opaque { get; private set; }

        public bool Completed { get; set; }

        public InterChunk()
        {
            Opaque = new InterChunkMesh();
            Translucent = new InterChunkMesh();
        }
    }
}
