#region Using

using System;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkMesh
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        public ChunkMeshPart Opaque { get; private set; }

        public ChunkMeshPart Translucent { get; private set; }

        public ChunkMesh(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            GraphicsDevice = graphicsDevice;

            Translucent = new ChunkMeshPart(graphicsDevice);
            Opaque = new ChunkMeshPart(graphicsDevice);
        }
    }
}
