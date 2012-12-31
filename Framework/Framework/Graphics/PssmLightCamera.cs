#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class PssmLightCamera : ICamera
    {
        public Matrix LightViewProjection = Matrix.Identity;

        int shadowMapSize;

        // 分割視錐台の頂点のキャッシュ。
        Vector3[] frustumCorners = new Vector3[8];

        // 投影オブジェクト判定で用いる作業配列。
        Vector3[] casterBoxCorners = new Vector3[8];

        List<Vector3> lightVolumePoints;

        View lightView;

        Orthograph lightProjection;

        // I/F
        public string Name { get; private set; }

        // I/F
        public View View { get; private set; }

        // I/F
        public PerspectiveFov Projection { get; private set; }

        // I/F
        public BoundingFrustum Frustum { get; private set; }

        public Vector3 LightDirection
        {
            get { return lightView.Direction; }
            set { lightView.Direction = value; }
        }

        public PssmLightCamera(string name, int shadowMapSize)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (shadowMapSize < 1) throw new ArgumentOutOfRangeException("shadowMapSize");

            Name = name;
            this.shadowMapSize = shadowMapSize;

            // カメラ。
            View = new View();
            Projection = new PerspectiveFov
            {
                AspectRatio = PerspectiveFov.AspectRatio1x1
            };
            Frustum = new BoundingFrustum(Matrix.Identity);

            // ライト視点。
            lightView = new View();
            lightProjection = new Orthograph();

            // TODO: 初期容量
            lightVolumePoints = new List<Vector3>();
        }

        // I/F
        public void Update()
        {
            // ビュー行列と射影行列を更新。
            View.Update();
            Projection.Update();

            // 視錐台。
            Matrix viewProjection;
            Matrix.Multiply(ref View.Matrix, ref Projection.Matrix, out viewProjection);
            Frustum.Matrix = viewProjection;

            // 頂点をキャッシュ。
            Frustum.GetCorners(frustumCorners);
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

        public void UpdateLightViewProjection()
        {
            //----------------------------------------------------------------
            // 光源のビュー行列を更新

            // 分割視錐台の中心
            var frustumCenter = Vector3.Zero;
            for (int i = 0; i < 8; i++) frustumCenter += frustumCorners[i];
            frustumCenter /= 8;

            // 分割視錐台の中心からどの程度の距離に光源を位置させるか
            float farExtent;
            Vector3.Distance(ref frustumCorners[4], ref frustumCorners[5], out farExtent);
            var centerDistance = MathHelper.Max(Projection.FarPlaneDistance - Projection.NearPlaneDistance, farExtent);

            // 光源位置を算出して設定
            lightView.Position = frustumCenter - lightView.Direction * centerDistance;
            
            // 行列を更新
            lightView.Update();

            //----------------------------------------------------------------
            // 包含座標を全て含む領域を算出

            Vector3 initialPointLS;
            Vector3.Transform(ref frustumCorners[0], ref lightView.Matrix, out initialPointLS);
            var min = initialPointLS;
            var max = initialPointLS;

            // 分割視錐台の頂点を包含座標として判定。
            for (int i = 0; i < 8; i++)
            {
                Vector3 pointWS = frustumCorners[i];
                Vector3 pointLS;
                Vector3.Transform(ref pointWS, ref lightView.Matrix, out pointLS);

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
                Vector3.Transform(ref pointWS, ref lightView.Matrix, out pointLS);

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
            // 包含座標を全て含む領域を基に、光源の射影行列を更新。
            // REFERECE: http://msdn.microsoft.com/ja-jp/library/ee416324(VS.85).aspx

            var texelSize = 1.0f / (float) shadowMapSize;
            lightProjection.Left = AdjustProjectionBoundary(min.X, texelSize);
            lightProjection.Right = AdjustProjectionBoundary(max.X, texelSize);
            lightProjection.Bottom = AdjustProjectionBoundary(min.Y, texelSize);
            lightProjection.Top = AdjustProjectionBoundary(max.Y, texelSize);
            lightProjection.ZNearPlane = -max.Z;
            lightProjection.ZFarPlane = -min.Z;
            lightProjection.Update();

            Matrix.Multiply(ref lightView.Matrix, ref lightProjection.Matrix, out LightViewProjection);

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
