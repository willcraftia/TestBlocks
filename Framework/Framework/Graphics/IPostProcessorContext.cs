#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public interface IPostProcessorContext
    {
        ICamera ActiveCamera { get; }

        ShadowMap ShadowMap { get; }

        List<SceneObject> OpaqueObjects { get; }

        List<SceneObject> TranslucentObjects { get; }

        RenderTarget2D Source { get; }

        RenderTarget2D Destination { get; }
    }
}
