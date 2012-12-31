#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public interface ICamera
    {
        string Name { get; }

        View View { get; }

        PerspectiveFov Projection { get; }

        BoundingFrustum Frustum { get; }

        void Update();
    }
}
