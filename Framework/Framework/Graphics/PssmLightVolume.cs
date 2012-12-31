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

        float shadowMapTexelSize;

        Vector3[] frustumCorners = new Vector3[8];

        // TODO: 初期容量
        List<Vector3> lightVolumePoints = new List<Vector3>();

        public PssmLightVolume(int shadowMapSize)
        {
            if (shadowMapSize < 1) throw new ArgumentOutOfRangeException("shadowMapSize");

            this.shadowMapSize = shadowMapSize;
        
            shadowMapTexelSize = 1 / (float) shadowMapSize;
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
            //----------------------------------------------------------------
            // 光源のビュー行列を更新

            // 分割視錐台の中心。
            //var frustumCenter = Vector3.Zero;
            //for (int i = 0; i < 8; i++) frustumCenter += frustumCorners[i];
            //frustumCenter /= 8;

            // 分割視錐台の中心からどの程度の距離に光源を位置させるか。
            //float farExtent;
            //Vector3.Distance(ref frustumCorners[4], ref frustumCorners[5], out farExtent);
            //var centerDistance = MathHelper.Max(camera.Projection.FarPlaneDistance - camera.Projection.NearPlaneDistance, farExtent);

            //float centerDistance;
            //Vector3.Distance(ref frustumCorners[2], ref frustumCorners[4], out centerDistance);

            // 視錐台を含む球の球面にカメラがあれば良いと考えたが・・・
            // 球面にある光源のビュー座標へ視錐台や追加の包含座標を変換し、
            // それらを全て含む光源ビュー座標での AABB から正射影行列を作れば、
            // そこに全てが収まると考えた。
            var sphere = BoundingSphere.CreateFromFrustum(camera.Frustum);
            var centerDistance = sphere.Radius;
            var frustumCenter = sphere.Center;

            // ライトのビュー行列。
            var lightPosition = frustumCenter - lightDirection * centerDistance;
            var lightTarget = lightPosition + lightDirection;
            var lightUp = Vector3.Up;
            Matrix lightView;
            Matrix.CreateLookAt(ref lightPosition, ref lightTarget, ref lightUp, out lightView);

            //----------------------------------------------------------------
            // ライト ボリューム算出

            var lightVolume = BoundingBoxHelper.Empty;

            // 分割視錐台の頂点を包含座標として判定。
            camera.Frustum.GetCorners(frustumCorners);
            for (int i = 0; i < 8; i++)
            {
                Vector3 pointWS = frustumCorners[i];
                Vector3 pointLS;
                Vector3.Transform(ref pointWS, ref lightView, out pointLS);

                Vector3.Min(ref lightVolume.Min, ref pointLS, out lightVolume.Min);
                Vector3.Max(ref lightVolume.Max, ref pointLS, out lightVolume.Max);
            }

            // 投影オブジェクトの追加などで検知した座標を包含座標として判定。
            for (int i = 0; i < lightVolumePoints.Count; i++)
            {
                Vector3 pointWS = lightVolumePoints[i];
                Vector3 pointLS;
                Vector3.Transform(ref pointWS, ref lightView, out pointLS);

                Vector3.Min(ref lightVolume.Min, ref pointLS, out lightVolume.Min);
                Vector3.Max(ref lightVolume.Max, ref pointLS, out lightVolume.Max);
            }

            // シャドウ マップのサイズにあわせてサイズを微調整。
            Adjust(ref lightVolume);

            //----------------------------------------------------------------
            // ライト ボリュームの射影行列
            //
            // REFERECE: http://msdn.microsoft.com/ja-jp/library/ee416324(VS.85).aspx

            Matrix lightProjection;
            Matrix.CreateOrthographicOffCenter(
                lightVolume.Min.X, lightVolume.Max.X,
                lightVolume.Min.Y, lightVolume.Max.Y,
                -lightVolume.Max.Z, -lightVolume.Min.Z,
                out lightProjection);

            //----------------------------------------------------------------
            // ライト ボリュームのビュー×射影行列

            Matrix.Multiply(ref lightView, ref lightProjection, out LightViewProjection);

            // クリア。
            lightVolumePoints.Clear();
        }

        void Adjust(ref BoundingBox lightVolume)
        {
            lightVolume.Min.X = Adjust(lightVolume.Min.X);
            lightVolume.Min.Y = Adjust(lightVolume.Min.Y);
            lightVolume.Min.Z = Adjust(lightVolume.Min.Z);

            lightVolume.Max.X = Adjust(lightVolume.Max.X);
            lightVolume.Max.Y = Adjust(lightVolume.Max.Y);
            lightVolume.Max.Z = Adjust(lightVolume.Max.Z);
        }

        float Adjust(float value)
        {
            var result = value;
            result *= shadowMapSize;
            result = MathExtension.Floor(result);
            result *= shadowMapTexelSize;
            return result;
        }
    }
}
