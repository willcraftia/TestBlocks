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

        public ChunkMesh(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            Translucent = new ChunkMeshPart(graphicsDevice);
            Opaque = new ChunkMeshPart(graphicsDevice);
        }
    }
}
