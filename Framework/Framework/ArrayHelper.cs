#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework
{
    public static class ArrayHelper
    {
        public static bool IsNullOrEmpty<T>(T[] array)
        {
            return array == null || array.Length == 0;
        }
    }
}
