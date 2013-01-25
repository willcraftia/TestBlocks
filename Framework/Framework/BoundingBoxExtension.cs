#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework
{
    public static class BoundingBoxExtension
    {
        public static Vector3 GetCenter(this BoundingBox box)
        {
            Vector3 result;
            box.GetCenter(out result);
            return result;
        }

        public static void GetCenter(this BoundingBox box, out Vector3 result)
        {
            // 参考: result = (box.Max + box.Min) / 2
            
            Vector3 maxMin;
            Vector3.Add(ref box.Max, ref box.Max, out maxMin);
            Vector3.Divide(ref maxMin, 2, out result);
        }

        public static Vector3 GetSize(this BoundingBox box)
        {
            Vector3 result;
            box.GetSize(out result);
            return result;
        }

        public static void GetSize(this BoundingBox box, out Vector3 result)
        {
            // 参考: result = box.Max - box.Min

            Vector3.Subtract(ref box.Max, ref box.Min, out result);
        }

        public static Vector3 GetHalfSize(this BoundingBox box)
        {
            Vector3 result;
            box.GetHalfSize(out result);
            return result;
        }

        public static void GetHalfSize(this BoundingBox box, out Vector3 result)
        {
            // 参考: result = (box.Max - box.Min) / 2

            Vector3 size;
            Vector3.Subtract(ref box.Max, ref box.Min, out size);
            Vector3.Divide(ref size, 2, out result);
        }
    }
}
