﻿#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public interface ISceneModuleFactory
    {
        Effect CreateGaussianBlurEffect();

        Effect CreateShadowMapEffect();

        Pssm CreatePssm(ShadowSettings shadowSettings);

#if DEBUG
        BasicEffect CreateDebugBoundingBoxEffect();

        BoundingBoxDrawer CreateDebugBoundingBoxDrawer();
#endif
    }
}