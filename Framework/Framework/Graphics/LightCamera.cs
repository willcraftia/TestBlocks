#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class LightCamera
    {
        public Matrix LightViewProjection = Matrix.Identity;

        Vector3[] frustumCorners = new Vector3[8];

        Vector3[] boundingBoxCorners = new Vector3[8];

        // TODO: 初期容量
        List<Vector3> lightVolumePoints = new List<Vector3>();

        int shadowMapSize;

        float shadowMapTexelSize;

        public LightCamera(int shadowMapSize)
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
            var position = Vector3.Zero;
            var target = lightDirection;
            var up = Vector3.Up;
            
            Matrix tempLightView;
            Matrix.CreateLookAt(ref position, ref target, ref up, out tempLightView);

            camera.Frustum.GetCorners(frustumCorners);
            AddLightVolumePoints(frustumCorners);

            var tempLightVolume = BoundingBox.CreateFromPoints(lightVolumePoints);
            tempLightVolume.GetCorners(boundingBoxCorners);
            for (int i = 0; i < boundingBoxCorners.Length; i++)
                Vector3.Transform(ref boundingBoxCorners[i], ref tempLightView, out boundingBoxCorners[i]);

            var lightVolume = BoundingBox.CreateFromPoints(boundingBoxCorners);

            var boxSize = lightVolume.Max - lightVolume.Min;
            var halfBoxSize = boxSize * 0.5f;

            // 仮ライト空間での仮光源位置を算出。
            var lightPosition = lightVolume.Min + halfBoxSize;
            lightPosition.Z = lightVolume.Min.Z;

            // 仮光源位置を仮ライト空間からワールド空間へ変換。
            Matrix lightViewInv;
            Matrix.Invert(ref tempLightView, out lightViewInv);
            Vector3.Transform(ref lightPosition, ref lightViewInv, out lightPosition);

            target = lightPosition + lightDirection;

            Matrix lightView;
            Matrix.CreateLookAt(ref lightPosition, ref target, ref up, out lightView);

            // REFERECE: http://msdn.microsoft.com/ja-jp/library/ee416324(VS.85).aspx
            boxSize.X = Adjust(boxSize.X);
            boxSize.Y = Adjust(boxSize.Y);
            boxSize.Z = Adjust(boxSize.Z);

            Matrix lightProjection;
            Matrix.CreateOrthographic(boxSize.X, boxSize.Y, -boxSize.Z, boxSize.Z, out lightProjection);

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
