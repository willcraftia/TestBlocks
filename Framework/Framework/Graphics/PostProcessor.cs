#region Using

using System;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public abstract class PostProcessor
    {
        public bool Enabled { get; set; }

        protected SpriteBatch SpriteBatch { get; private set; }

        protected GraphicsDevice GraphicsDevice { get; private set; }

        protected PostProcessor(SpriteBatch spriteBatch)
        {
            if (spriteBatch == null) throw new ArgumentNullException("spriteBatch");

            Enabled = true;

            SpriteBatch = spriteBatch;
            GraphicsDevice = spriteBatch.GraphicsDevice;
        }

        public abstract void Process(IPostProcessorContext context);
    }
}
