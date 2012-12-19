#region Using

using System;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkMesh : IDisposable
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        public ChunkMeshPart Translucent { get; private set; }

        public ChunkMeshPart Opaque { get; private set; }

        public bool IsLoaded { get; set; }

        public ChunkMesh(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            GraphicsDevice = graphicsDevice;

            Translucent = new ChunkMeshPart(graphicsDevice);
            Opaque = new ChunkMeshPart(graphicsDevice);
        }

        public void BuildBuffers()
        {
            Translucent.BuildBuffer();
            Opaque.BuildBuffer();
        }

        public void Clear()
        {
            Translucent.Clear();
            Opaque.Clear();
            IsLoaded = false;
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~ChunkMesh()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            Translucent.Dispose();
            Opaque.Dispose();

            disposed = true;
        }

        #endregion
    }
}
