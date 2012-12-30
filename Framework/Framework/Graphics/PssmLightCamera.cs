#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class PssmLightCamera
    {
        public Matrix ViewProjection = Matrix.Identity;

        int shadowMapSize;

        // 分割視錐台の頂点のキャッシュ。
        Vector3[] splitEyeFrustumCorners = new Vector3[8];

        // 投影オブジェクト判定で用いる作業配列。
        Vector3[] casterBoxCorners = new Vector3[8];

        List<Vector3> lightVolumePoints;

        public LightView LightView { get; private set; }

        public Orthograph LightProjection { get; private set; }

        public BoundingFrustum SplitEyeFrustum { get; private set; }

        public PerspectiveFov SplitEyeProjection { get; private set; }

        public PssmLightCamera(int shadowMapSize)
        {
            this.shadowMapSize = shadowMapSize;

            LightView = new LightView();
            LightProjection = new Orthograph();
            SplitEyeFrustum = new BoundingFrustum(Matrix.Identity);
            SplitEyeProjection = new PerspectiveFov
            {
                AspectRatio = PerspectiveFov.AspectRatio1x1
            };

            // TODO: 初期容量
            lightVolumePoints = new List<Vector3>();
        }

        public void AddLightVolumePoint(ref Vector3 point)
        {
            lightVolumePoints.Add(point);
        }

        public void AddLightVolumePoints(Vector3[] points)
        {
            if (points == null) throw new ArgumentNullException("points");

            for (int i = 0; i < points.Length; i++)
                lightVolumePoints.Add(points[i]);
        }

        public void Prepare(ICamera camera)
        {
            // 分割視点の射影行列を更新。
            SplitEyeProjection.Update();

            // 視点のビュー行列と分割視点の射影行列で視錐台を構築。
            Matrix splitViewProjection;
            Matrix.Multiply(ref camera.View.Matrix, ref SplitEyeProjection.Matrix, out splitViewProjection);
            SplitEyeFrustum.Matrix = splitViewProjection;

            // 頂点をキャッシュ。
            SplitEyeFrustum.GetCorners(splitEyeFrustumCorners);
        }

        public void UpdateViewProjection()
        {
            //----------------------------------------------------------------
            // 光源のビュー行列を更新

            LightView.Update();

            //----------------------------------------------------------------
            // 包含座標を全て含む領域を算出

            Vector3 initialPointLS;
            Vector3.Transform(ref splitEyeFrustumCorners[0], ref LightView.Matrix, out initialPointLS);
            var min = initialPointLS;
            var max = initialPointLS;

            // 分割視錐台の頂点を包含座標として判定。
            for (int i = 0; i < 8; i++)
            {
                Vector3 pointWS = splitEyeFrustumCorners[i];
                Vector3 pointLS;
                Vector3.Transform(ref pointWS, ref LightView.Matrix, out pointLS);
                if (max.X < pointLS.X)
                {
                    max.X = pointLS.X;
                }
                else if (pointLS.X < min.X)
                {
                    min.X = pointLS.X;
                }
                if (max.Y < pointLS.Y)
                {
                    max.Y = pointLS.Y;
                }
                else if (pointLS.Y < min.Y)
                {
                    min.Y = pointLS.Y;
                }
                if (max.Z < pointLS.Z)
                {
                    max.Z = pointLS.Z;
                }
                else if (pointLS.Z < min.Z)
                {
                    min.Z = pointLS.Z;
                }
            }
            // 投影オブジェクトの追加などで検知した座標を包含座標として判定。
            for (int i = 0; i < lightVolumePoints.Count; i++)
            {
                Vector3 pointWS = lightVolumePoints[i];
                Vector3 pointLS;
                Vector3.Transform(ref pointWS, ref LightView.Matrix, out pointLS);
                if (max.X < pointLS.X)
                {
                    max.X = pointLS.X;
                }
                else if (pointLS.X < min.X)
                {
                    min.X = pointLS.X;
                }
                if (max.Y < pointLS.Y)
                {
                    max.Y = pointLS.Y;
                }
                else if (pointLS.Y < min.Y)
                {
                    min.Y = pointLS.Y;
                }
                if (max.Z < pointLS.Z)
                {
                    max.Z = pointLS.Z;
                }
                else if (pointLS.Z < min.Z)
                {
                    min.Z = pointLS.Z;
                }
            }

            //----------------------------------------------------------------
            // 包含座標を全て含む領域を基に、光源の射影行列を更新。
            // REFERECE: http://msdn.microsoft.com/ja-jp/library/ee416324(VS.85).aspx

            var texelSize = 1.0f / (float) shadowMapSize;
            LightProjection.Left = AdjustProjectionBoundary(min.X, texelSize);
            LightProjection.Right = AdjustProjectionBoundary(max.X, texelSize);
            LightProjection.Bottom = AdjustProjectionBoundary(min.Y, texelSize);
            LightProjection.Top = AdjustProjectionBoundary(max.Y, texelSize);
            LightProjection.ZNearPlane = -max.Z;
            LightProjection.ZFarPlane = -min.Z;
            LightProjection.Update();

            Matrix.Multiply(ref LightView.Matrix, ref LightProjection.Matrix, out ViewProjection);

            // クリア。
            lightVolumePoints.Clear();
        }

        float AdjustProjectionBoundary(float value, float texelSize)
        {
            var result = value;
            //result /= texelSize;
            result *= shadowMapSize;
            result = MathExtension.Floor(result);
            result *= texelSize;
            return result;
        }
    }
}
