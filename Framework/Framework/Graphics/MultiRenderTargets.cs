#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class MultiRenderTargets : IDisposable
    {
        RenderTarget2D[] renderTargets;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public string Name { get; private set; }

        public int Count { get; private set; }

        public int Width
        {
            get { return renderTargets[0].Width; }
        }

        public int Height
        {
            get { return renderTargets[0].Height; }
        }

        public SurfaceFormat Format
        {
            get { return renderTargets[0].Format; }
        }

        public DepthFormat DepthStencilFormat
        {
            get { return renderTargets[0].DepthStencilFormat; }
        }

        public RenderTargetUsage RenderTargetUsage
        {
            get { return renderTargets[0].RenderTargetUsage; }
        }

        public int MultiSampleCount
        {
            get { return renderTargets[0].MultiSampleCount; }
        }

        public RenderTarget2D this[int index]
        {
            get
            {
                if (index < 0 || Count < index) throw new ArgumentOutOfRangeException("index");

                return renderTargets[index];
            }
        }

        public Rectangle Bounds
        {
            get { return new Rectangle(0, 0, Width, Height); }
        }

        public MultiRenderTargets(GraphicsDevice graphicsDevice, string name, int renderTargetCount,
            int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat,
            int preferredMultiSampleCount, RenderTargetUsage usage)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (renderTargetCount < 1) throw new ArgumentOutOfRangeException("renderTargetCount");
            if (width < 1) throw new ArgumentOutOfRangeException("width");
            if (height < 1) throw new ArgumentOutOfRangeException("height");
            if (preferredMultiSampleCount < 0) throw new ArgumentOutOfRangeException("preferredMultiSampleCount");

            GraphicsDevice = graphicsDevice;
            Name = name;
            Count = renderTargetCount;

            renderTargets = new RenderTarget2D[renderTargetCount];

            for (int i = 0; i < renderTargetCount; i++)
            {
                renderTargets[i] = new RenderTarget2D(
                    graphicsDevice, width, height,
                    mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage);
                renderTargets[i].Name = CreateRenderTargetName(i);
            }
        }

        string CreateRenderTargetName(int index)
        {
            if (string.IsNullOrEmpty(Name)) return null;

            if (Count == 0) return Name;
            return Name + "." + index;
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~MultiRenderTargets()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                for (int i = 0; i < renderTargets.Length; i++)
                {
                    if (renderTargets[i] != null) renderTargets[i].Dispose();
                }
            }

            disposed = true;
        }

        #endregion
    }
}
