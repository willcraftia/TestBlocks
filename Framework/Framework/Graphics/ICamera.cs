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

        Vector3 Position { get; }

        Vector3 Forward { get; }

        Vector3 Up { get; }

        void Update();
    }
}
