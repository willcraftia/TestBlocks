#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public interface ISceneModuleFactory
    {
        Effect CreateShadowMapEffect();

        Effect CreateShadowSceneEffect();

        Effect CreateSssmEffect();

        Effect CreateDepthMapEffect();

        Effect CreateDofEffect();

        Effect CreateGaussianBlurEffect();

        Effect CreateNormalDepthMapEffect();

        Effect CreateEdgeEffect();
    }
}
