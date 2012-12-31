#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework
{
    /// <summary>
    /// ビュー行列を管理するクラスです。
    /// </summary>
    public sealed class View
    {
        /// <summary>
        /// ビュー行列。
        /// </summary>
        public Matrix Matrix = Matrix.Identity;

        Vector3 position = Vector3.Zero;

        Vector3 direction = Matrix.Identity.Forward;

        Vector3 up = Matrix.Identity.Up;

        bool matrixDirty = true;

        /// <summary>
        /// 視点位置を取得または設定します。
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set
            {
                if (position == value) return;

                position = value;
                matrixDirty = true;
            }
        }

        /// <summary>
        /// 視線方向を取得または設定します。
        /// </summary>
        public Vector3 Direction
        {
            get { return direction; }
            set
            {
                if (direction == value) return;

                direction = value;
                matrixDirty = true;
            }
        }

        /// <summary>
        /// 視線の Up ベクトル を取得または設定します。
        /// </summary>
        public Vector3 Up
        {
            get { return up; }
            set
            {
                if (up == value) return;

                up = value;
                matrixDirty = true;
            }
        }

        /// <summary>
        /// ビュー行列を更新します。
        /// </summary>
        public void Update()
        {
            if (!matrixDirty) return;

            var target = position + direction;
            Matrix.CreateLookAt(ref position, ref target, ref up, out Matrix);

            matrixDirty = false;
        }

        public static Vector3 GetPosition(Matrix view)
        {
            Vector3 result;
            GetPosition(ref view, out result);
            return result;
        }

        public static void GetPosition(ref Matrix view, out Vector3 result)
        {
            Matrix inverse;
            Matrix.Invert(ref view, out inverse);
            result = inverse.Translation;
        }

        public static Vector3 GetDirection(Matrix view)
        {
            Vector3 result;
            GetDirection(ref view, out result);
            return result;
        }

        public static void GetDirection(ref Matrix view, out Vector3 result)
        {
            Matrix inverse;
            Matrix.Invert(ref view, out inverse);
            result = inverse.Forward;
        }

        public static Vector3 GetUp(Matrix view)
        {
            Vector3 result;
            GetUp(ref view, out result);
            return result;
        }

        public static void GetUp(ref Matrix view, out Vector3 result)
        {
            Matrix inverse;
            Matrix.Invert(ref view, out inverse);
            result = inverse.Up;
        }
    }
}
