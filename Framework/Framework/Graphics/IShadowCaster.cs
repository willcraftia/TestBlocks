#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public interface IShadowCaster
    {
        void GetShadowTestBoundingSphere(out BoundingSphere sphere);

        void GetShadowTestBoundingBox(out BoundingBox box);

        void Draw(Effect effect);
    }
}
