#region Using

using System;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public interface ISceneObjectContext
    {
        ICamera ActiveCamera { get; }

        DirectionalLight ActiveDirectionalLight { get; }
    }
}
