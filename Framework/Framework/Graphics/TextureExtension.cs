#region Using

using System;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public static class TextureExtension
    {
        public static SamplerState GetPreferredSamplerState(this Texture texture)
        {
            switch (texture.Format)
            {
                case SurfaceFormat.Single:
                case SurfaceFormat.HalfSingle:
                case SurfaceFormat.Vector2:
                case SurfaceFormat.Vector4:
                    return SamplerState.PointClamp;
            }

            return null;
        }
    }
}
