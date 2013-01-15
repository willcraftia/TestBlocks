#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class InterChunk
    {
        volatile bool completed;

        public InterChunkMesh Translucent { get; private set; }

        public InterChunkMesh Opaque { get; private set; }

        public bool Completed
        {
            get { return completed; }
            set { completed = value; }
        }

        public InterChunk()
        {
            Opaque = new InterChunkMesh();
            Translucent = new InterChunkMesh();
        }
    }
}
