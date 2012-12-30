#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class MultiRenderTargets : IDisposable
    {
        int currentIndex;
        
        RenderTarget2D[] renderTargets;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public string Name { get; private set; }

        public int RenderTargetCount { get; private set; }

        public int Width
        {
            get { return Current.Width; }
        }

        public int Height
        {
            get { return Current.Height; }
        }

        public SurfaceFormat Format
        {
            get { return Current.Format; }
        }

        public DepthFormat DepthStencilFormat
        {
            get { return Current.DepthStencilFormat; }
        }

        public RenderTargetUsage RenderTargetUsage
        {
            get { return Current.RenderTargetUsage; }
        }

        public int MultiSampleCount
        {
            get { return Current.MultiSampleCount; }
        }

        public int CurrentIndex
        {
            get { return currentIndex; }
            set
            {
                if (value < 0 || RenderTargetCount <= value)
                    throw new ArgumentOutOfRangeException("CurrentIndex");

                currentIndex = value;
            }
        }

        public RenderTarget2D this[int index]
        {
            get
            {
                return renderTargets[index];
            }
        }

        public RenderTarget2D Current
        {
            get { return renderTargets[currentIndex]; }
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
            RenderTargetCount = renderTargetCount;

            currentIndex = 0;

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

            if (RenderTargetCount == 0) return Name;
            return Name + "." + index;
        }

        void DisposeRenderTargets()
        {
            if (renderTargets == null) return;

            for (int i = 0; i < renderTargets.Length; i++)
            {
                if (renderTargets[i] != null) renderTargets[i].Dispose();
            }
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
                DisposeRenderTargets();
            }

            disposed = true;
        }

        #endregion
    }
}
