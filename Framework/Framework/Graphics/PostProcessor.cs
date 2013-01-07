#region Using

using System;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public abstract class PostProcessor
    {
        public abstract void Process(IPostProcessorContext context, RenderTarget2D source, RenderTarget2D destination);
    }
}
