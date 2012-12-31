#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class PssmLightVolume
    {
        public Matrix LightViewProjection = Matrix.Identity;

        int shadowMapSize;

        Vector3[] frustumCorners = new Vector3[8];

        // TODO: 初期容量
        List<Vector3> lightVolumePoints = new List<Vector3>();

        public PssmLightVolume(int shadowMapSize)
        {
            if (shadowMapSize < 1) throw new ArgumentOutOfRangeException("shadowMapSize");

            this.shadowMapSize = shadowMapSize;
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

        public void Update(ICamera camera, ref Vector3 lightDirection)
        {
            camera.Frustum.GetCorners(frustumCorners);

            //----------------------------------------------------------------
            // 光源のビュー行列を更新

            // 分割視錐台の中心。
            var frustumCenter = Vector3.Zero;
            for (int i = 0; i < 8; i++) frustumCenter += frustumCorners[i];
            frustumCenter /= 8;

            // 分割視錐台の中心からどの程度の距離に光源を位置させるか。
            float farExtent;
            Vector3.Distance(ref frustumCorners[4], ref frustumCorners[5], out farExtent);
            var centerDistance = MathHelper.Max(camera.Projection.FarPlaneDistance - camera.Projection.NearPlaneDistance, farExtent);

            // ライトのビュー行列。
            var lightPosition = frustumCenter - lightDirection * centerDistance;
            var lightTarget = lightPosition + lightDirection;
            var lightUp = Vector3.Up;
            Matrix lightView;
            Matrix.CreateLookAt(ref lightPosition, ref lightTarget, ref lightUp, out lightView);

            //----------------------------------------------------------------
            // 包含座標を全て含む領域を算出

            Vector3 initialPointLS;
            Vector3.Transform(ref frustumCorners[0], ref lightView, out initialPointLS);
            var min = initialPointLS;
            var max = initialPointLS;

            // 分割視錐台の頂点を包含座標として判定。
            for (int i = 0; i < 8; i++)
            {
                Vector3 pointWS = frustumCorners[i];
                Vector3 pointLS;
                Vector3.Transform(ref pointWS, ref lightView, out pointLS);

                // TODO: Vector3.Min/Max で良いのでは？
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
                Vector3.Transform(ref pointWS, ref lightView, out pointLS);

                // TODO: Vector3.Min/Max で良いのでは？
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
            // 包含座標を全て含む領域を基に、光源の射影行列。
            // REFERECE: http://msdn.microsoft.com/ja-jp/library/ee416324(VS.85).aspx

            var texelSize = 1.0f / (float) shadowMapSize;
            var left = AdjustProjectionBoundary(min.X, texelSize);
            var right = AdjustProjectionBoundary(max.X, texelSize);
            var bottom = AdjustProjectionBoundary(min.Y, texelSize);
            var Top = AdjustProjectionBoundary(max.Y, texelSize);
            var zNearPlane = -max.Z;
            var zFarPlane = -min.Z;
            Matrix lightProjection;
            Matrix.CreateOrthographicOffCenter(left, right, bottom, Top, zNearPlane, zFarPlane, out lightProjection);

            Matrix.Multiply(ref lightView, ref lightProjection, out LightViewProjection);

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
