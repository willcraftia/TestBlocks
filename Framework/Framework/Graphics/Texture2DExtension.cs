#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public static class Texture2DExtension
    {
        public static Rectangle GetBounds(this Texture2D texture)
        {
            return new Rectangle(0, 0, texture.Width, texture.Height);
        }

        public static void GetBounds(this Texture2D texture, out Rectangle result)
        {
            result = new Rectangle(0, 0, texture.Width, texture.Height);
        }
    }
}
