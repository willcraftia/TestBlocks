#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class PssmLightCamera
    {
        // 光源のビュー行列×射影行列。
        public Matrix Matrix = Matrix.Identity;

        // 分割視錐台の頂点のキャッシュ。
        Vector3[] splitEyeFrustumCorners = new Vector3[8];

        //
        // TODO: 初期容量
        //
        List<Vector3> lightVolumePoints = new List<Vector3>();

        // このカメラで描画すべき投影オブジェクトのリスト。
        List<IShadowCaster> shadowCasters = new List<IShadowCaster>();

        // 投影オブジェクト判定で用いる作業配列。
        Vector3[] casterBoxCorners = new Vector3[8];

        public LightView View { get; private set; }

        public Orthograph Projection { get; private set; }

        public BoundingFrustum SplitEyeFrustum { get; private set; }

        public PerspectiveFov SplitEyeProjection { get; private set; }

        public int ShadowMapSize { get; set; }

        public IEnumerable<IShadowCaster> ShadowCasters
        {
            get { return shadowCasters; }
        }

        public PssmLightCamera()
        {
            Projection = new Orthograph();
            SplitEyeFrustum = new BoundingFrustum(Matrix.Identity);
            SplitEyeProjection = new PerspectiveFov
            {
                AspectRatio = PerspectiveFov.AspectRatio1x1
            };
        }

        public void Prepare(View eyeView)
        {
            // 分割視点カメラの射影行列を更新。
            SplitEyeProjection.Update();

            // 視点カメラのビュー行列と分割視点カメラの射影行列で視錐台を構築。
            Matrix splitViewProjection;
            Matrix.Multiply(ref eyeView.Matrix, ref SplitEyeProjection.Matrix, out splitViewProjection);
            SplitEyeFrustum.Matrix = splitViewProjection;

            // 頂点をキャッシュ。
            SplitEyeFrustum.GetCorners(splitEyeFrustumCorners);
        }

        // Prepare の後に呼び出される前提。
        public bool TryAddShadowCaster(IShadowCaster shadowCaster)
        {
            BoundingSphere casterSphere;
            shadowCaster.GetBoundingSphere(out casterSphere);
            
            BoundingBox casterBox;
            shadowCaster.GetBoundingBox(out casterBox);
            casterBox.GetCorners(casterBoxCorners);

            if (casterSphere.Intersects(SplitEyeFrustum))
            {
                // 投影オブジェクトとして登録。
                shadowCasters.Add(shadowCaster);

                // AABB の頂点を包含座標として登録。
                for (int i = 0; i < 8; i++)
                    lightVolumePoints.Add(casterBoxCorners[i]);

                return true;
            }
            else
            {
                // TODO: 要検討。
                for (int i = 0; i < 8; i++)
                {
                    // AABB の頂点から光方向の線。
                    var ray = new Ray(casterBoxCorners[i], View.Direction);

                    // 分割視錐台と交差するか否か。
                    var distance = ray.Intersects(SplitEyeFrustum);
                    if (distance == null) continue;

                    // TODO
                    if (distance < 10)
                    {
                        // 交差距離が一定値未満ならば投影オブジェクトとして登録。
                        shadowCasters.Add(shadowCaster);

                        // AABB の頂点を包含座標として登録。
                        for (int j = 0; j < 8; j++)
                            lightVolumePoints.Add(casterBoxCorners[i]);

                        return true;
                    }
                }
            }

            return false;
        }

        public void Update(View eyeView)
        {
            //----------------------------------------------------------------
            // 光源のビュー行列を更新

            View.Update();

            //----------------------------------------------------------------
            // 包含座標を全て含む領域を算出

            Vector3 initialPointLS;
            Vector3.Transform(ref splitEyeFrustumCorners[0], ref View.Matrix, out initialPointLS);
            var min = initialPointLS;
            var max = initialPointLS;

            // 分割視錐台の頂点を包含座標として判定。
            for (int i = 0; i < 8; i++)
            {
                Vector3 pointWS = splitEyeFrustumCorners[i];
                Vector3 pointLS;
                Vector3.Transform(ref pointWS, ref View.Matrix, out pointLS);
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
                Vector3.Transform(ref pointWS, ref View.Matrix, out pointLS);
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

            var texelSize = 1.0f / (float) ShadowMapSize;
            Projection.Left = AdjustProjectionBoundary(min.X, texelSize);
            Projection.Right = AdjustProjectionBoundary(max.X, texelSize);
            Projection.Bottom = AdjustProjectionBoundary(min.Y, texelSize);
            Projection.Top = AdjustProjectionBoundary(max.Y, texelSize);
            Projection.ZNearPlane = -max.Z;
            Projection.ZFarPlane = -min.Z;
            Projection.Update();

            //----------------------------------------------------------------
            // 光源のビュー行列×射影行列を更新

            Matrix.Multiply(ref View.Matrix, ref Projection.Matrix, out Matrix);
        }

        public void Clear()
        {
            lightVolumePoints.Clear();
            shadowCasters.Clear();
        }

        float AdjustProjectionBoundary(float value, float texelSize)
        {
            var result = value;
            result /= texelSize;
            result = (float) Math.Floor(result);
            result *= texelSize;
            return result;
        }
    }
}
