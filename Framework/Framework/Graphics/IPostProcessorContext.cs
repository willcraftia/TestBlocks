#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public interface IPostProcessorContext
    {
        ICamera ActiveCamera { get; }

        ShadowMap ShadowMap { get; }

        Vector3 ShadowColor { get; }

        IEnumerable<SceneObject> VisibleSceneObjects { get; }
    }
}
