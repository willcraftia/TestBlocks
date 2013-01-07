#region Using

using System;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public abstract class PostProcessor
    {
        #region PostProcessorMonitor

        public abstract class PostProcessorMonitor
        {
            public event EventHandler BeginProcess = delegate { };

            public event EventHandler EndProcess = delegate { };

            protected PostProcessor PostProcessor { get; private set; }

            protected PostProcessorMonitor(PostProcessor postProcessor)
            {
                if (postProcessor == null) throw new ArgumentNullException("postProcessor");

                PostProcessor = postProcessor;
            }

            internal void OnBeginProcess()
            {
                BeginProcess(PostProcessor, EventArgs.Empty);
            }

            internal void OnEndProcess()
            {
                EndProcess(PostProcessor, EventArgs.Empty);
            }
        }

        #endregion

        protected SpriteBatch SpriteBatch { get; private set; }

        protected GraphicsDevice GraphicsDevice { get; private set; }

        protected PostProcessor(SpriteBatch spriteBatch)
        {
            if (spriteBatch == null) throw new ArgumentNullException("spriteBatch");

            SpriteBatch = spriteBatch;
            GraphicsDevice = spriteBatch.GraphicsDevice;
        }

        public abstract void Process(IPostProcessorContext context, RenderTarget2D source, RenderTarget2D destination);
    }
}
