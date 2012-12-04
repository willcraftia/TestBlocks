#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkMesh
    {
        public ChunkMeshPart Translucent { get; private set; }

        public ChunkMeshPart Opaque { get; private set; }

        public bool IsLoaded { get; set; }

        public ChunkMesh()
        {
            Translucent = new ChunkMeshPart();
            Opaque = new ChunkMeshPart();
        }

        public void Clear()
        {
            Translucent.Clear();
            Opaque.Clear();
            IsLoaded = false;
        }
    }
}
