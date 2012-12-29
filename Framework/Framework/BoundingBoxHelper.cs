#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework
{
    public static class BoundingBoxHelper
    {
        public static BoundingBox Empty
        {
            get { return new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue)); }
        }
    }
}
