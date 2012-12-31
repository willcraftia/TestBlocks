#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public interface IEffectShadow
    {
        Matrix World { get; set; }

        void Apply();
    }
}
