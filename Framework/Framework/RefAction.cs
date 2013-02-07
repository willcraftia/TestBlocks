#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework
{
    public delegate void RefAction<T>(ref T point) where T : struct;
}
