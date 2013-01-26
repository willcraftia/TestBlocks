#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public abstract class ShadowCaster : SceneObject
    {
        public bool CastShadow { get; set; }

        protected ShadowCaster(string name)
            : base(name)
        {
            CastShadow = true;
        }
    }
}
