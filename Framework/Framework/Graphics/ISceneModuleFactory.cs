#region Using

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

        Effect CreatePssmSceneEffect();

        Effect CreateScreenSpaceShadowEffect();

        Pssm CreatePssm(ShadowSettings shadowSettings);

        PssmScene CreatePssmScene(ShadowSettings shadowSettings);

        ScreenSpaceShadow CreateScreenSpaceShadow(ShadowSceneSettings shadowSceneSettings);

        #region Debug

        BasicEffect CreateDebugBoundingBoxEffect();

        BoundingBoxDrawer CreateDebugBoundingBoxDrawer();

        #endregion
    }
}
