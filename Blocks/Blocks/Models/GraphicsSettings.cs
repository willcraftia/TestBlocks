#region Using

using System;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class GraphicsSettings
    {
        public ShadowMap.Settings ShadowMap { get; private set; }

        public GraphicsSettings()
        {
            ShadowMap = new ShadowMap.Settings();
        }
    }
}
