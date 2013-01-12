#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public interface ISceneObjectContext
    {
        ICamera ActiveCamera { get; }
    }
}
