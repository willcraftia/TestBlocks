#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class Pssm
    {
        public const int MinSplitCount = 1;

        public const int MaxSplitCount = 7;

        public const int DefaultCount = 3;

        public const float DefaultLambda = 0.7f;

        float lambda = DefaultLambda;

        Vector3 lightDirection = Vector3.Down;

        Vector3[] corners = new Vector3[8];

        float eyeFarPlaneDistance;

        float inverseSplitCount;

        PssmLightCamera[] cameras;

        float[] splitDistances;

        Matrix[] splitViewProjections;

        public int SplitCount { get; private set; }

        public float Lambda
        {
            get { return lambda; }
            set
            {
                if (value < 0 || 1 < value) throw new ArgumentOutOfRangeException("value");

                lambda = value;
            }
        }

        public Vector3 LightDirection
        {
            get { return lightDirection; }
            set { lightDirection = value; }
        }

        public float[] SplitDistances
        {
            get { return splitDistances; }
        }

        public Matrix[] SplitViewProjections
        {
            get
            {
                for (int i = 0; i < SplitCount; i++)
                    Matrix.Multiply(ref cameras[i].View.Matrix, ref cameras[i].Projection.Matrix, out splitViewProjections[i]);
                return splitViewProjections;
            }
        }

        public int ShadowMapSize { get; set; }

        public Pssm()
            : this(DefaultCount)
        {
        }

        public Pssm(int splitCount)
        {
            if (splitCount < MinSplitCount || MaxSplitCount < splitCount) throw new ArgumentOutOfRangeException("splitCount");

            SplitCount = splitCount;

            inverseSplitCount = 1.0f / (float) SplitCount;
            splitDistances = new float[SplitCount + 1];

            cameras = new PssmLightCamera[SplitCount];
            for (int i = 0; i < SplitCount; i++) cameras[i] = new PssmLightCamera();
        }

        public PssmLightCamera GetCamera(int index)
        {
            if (index < 0 || SplitCount <= index) throw new ArgumentOutOfRangeException("index");

            return cameras[index];
        }

        // PSSM で参照する視点の射影は、必ずしも実視点の物でなくとも良い。
        // 例えば、影を投げる範囲を抑えるために、実視点の射影よりも短い距離の射影を用いるなど。
        public void Prepare(View eyeView, PerspectiveFov eyeProjection, ref BoundingBox boundingBox)
        {
            CalculateEyeFarPlaneDistance(eyeView, eyeProjection, ref boundingBox);
            CalculateSplitDistances(eyeProjection);

            for (int i = 0; i < SplitCount; i++)
            {
                var near = splitDistances[i];
                var far = splitDistances[i + 1];

                cameras[i].ShadowMapSize = ShadowMapSize;
                cameras[i].View.Direction = lightDirection;
                cameras[i].SplitEyeProjection.Fov = eyeProjection.Fov;
                cameras[i].SplitEyeProjection.AspectRatio = eyeProjection.AspectRatio;
                cameras[i].SplitEyeProjection.NearPlaneDistance = near;
                cameras[i].SplitEyeProjection.FarPlaneDistance = far;
                cameras[i].Prepare(eyeView);
            }
        }

        public void TryAddShadowCaster(IShadowCaster shadowCaster)
        {
            for (int i = 0; i < SplitCount; i++)
            {
                if (cameras[i].TryAddShadowCaster(shadowCaster)) return;
            }
        }

        void CalculateEyeFarPlaneDistance(View eyeView, PerspectiveFov eyeProjection, ref BoundingBox boundingBox)
        {
            var viewMatrix = eyeView.Matrix;

            //
            // smaller z, more far
            // z == 0 means the point of view
            //
            var maxFar = 0.0f;
            boundingBox.GetCorners(corners);
            for (int i = 0; i < 8; i++)
            {
                var z =
                    corners[i].X * viewMatrix.M13 +
                    corners[i].Y * viewMatrix.M23 +
                    corners[i].Z * viewMatrix.M33 +
                    viewMatrix.M43;

                if (z < maxFar) maxFar = z;
            }

            eyeFarPlaneDistance = eyeProjection.NearPlaneDistance - maxFar;
        }

        void CalculateSplitDistances(PerspectiveFov eyeProjection)
        {
            var near = eyeProjection.NearPlaneDistance;
            var far = eyeFarPlaneDistance;
            var farNearRatio = far / near;

            for (int i = 0; i < SplitCount; i++)
            {
                float idm = i * inverseSplitCount;
                float log = (float) (near * Math.Pow(farNearRatio, idm));

                // REFERENCE: the version in the main PSSM paper
                float uniform = near + (far - near) * idm;
                // REFERENCE: the version (?) in some actual codes,
                // i think the following is a wrong formula.
                //float uniform = (near + idm) * (far - near);

                splitDistances[i] = log * lambda + uniform * (1.0f - lambda);
            }

            splitDistances[0] = near;
            splitDistances[SplitCount] = eyeFarPlaneDistance;
        }
    }
}
