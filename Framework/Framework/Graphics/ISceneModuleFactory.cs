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

        Effect CreatePssmShadowSceneEffect();

        Effect CreateUsmShadowSceneEffect();

        Effect CreateSssmEffect();

        Pssm CreatePssm(ShadowSettings shadowSettings);

        Usm CreateUsm(ShadowSettings shadowSettings);

        PssmShadowScene CreatePssmShadowScene(ShadowSettings shadowSettings);

        UsmShadowScene CreateUsmShadowScene(ShadowSettings shadowSettings);

        Sssm CreateSssm(SssmSettings sssmSettings);

        #region Debug

        BasicEffect CreateDebugBoundingBoxEffect();

        BoundingBoxDrawer CreateDebugBoundingBoxDrawer();

        #endregion
    }
}
