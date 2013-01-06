#region Using

using System;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public interface ISceneModuleFactory
    {
        ShadowMap CreateShadowMap(ShadowMapSettings shadowMapSettings);

        ShadowScene CreateShadowScene(ShadowSettings shadowSettings);

        Sssm CreateSssm(SssmSettings sssmSettings);

        Dof CreateDof(DofSettings dofSettings);

        #region Debug

        BasicEffect CreateDebugBoundingBoxEffect();

        BoundingBoxDrawer CreateDebugBoundingBoxDrawer();

        #endregion
    }
}
