#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public abstract class LightCamera
    {
        public Matrix ViewProjection = Matrix.Identity;
     
        //
        // TODO: 初期容量
        //

        public abstract Vector3 LightDirection { get; set; }

        public abstract View View { get; }

        public abstract Projection Projection { get; }

        public abstract void Prepare(ICamera camera);

        public abstract void UpdateViewProjection();
    }
}
