#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class SkySphere : IAsset, ISceneObject
    {
        //
        // TODO
        //
        // 後々、空テクスチャはバイオームで管理し、
        // バイオームの切り替わりと共に様相が変化するように修正したい。
        //

        SphereMesh sphereMesh;

        // 0.999f 以上くらいでほどほどの太陽の大きさとなる。
        float sunThreshold = 0.999f;

        bool sunVisible = true;

        // I/F
        public IResource Resource { get; set; }

        // I/F
        public ISceneObjectContext Context { private get; set; }

        // I/F
        public bool Visible { get; set; }

        // I/F
        public bool Translucent
        {
            get { return false; }
        }

        // I/F
        public bool Occluded
        {
            get { return false; }
        }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public SkySphereEffect Effect { get; set; }

        public float SunThreshold
        {
            get { return sunThreshold; }
            set { sunThreshold = value; }
        }

        public bool SunVisible
        {
            get { return sunVisible; }
            set { sunVisible = value; }
        }

        public SceneSettings SceneSettings { get; set; }

        public SkySphere(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            GraphicsDevice = graphicsDevice;
            
            sphereMesh = new SphereMesh(GraphicsDevice);
        }

        // I/F
        public void GetDistanceSquared(ref Vector3 eyePosition, out float result)
        {
            // 常に再遠に位置させる。
            result = float.MaxValue;
        }

        // I/F
        public void GetBoundingSphere(out BoundingSphere result)
        {
            result = new BoundingSphere(Context.ActiveCamera.Position, CalculateRadius());
        }

        // I/F
        public void GetBoundingBox(out BoundingBox result)
        {
            BoundingSphere sphere;
            GetBoundingSphere(out sphere);
            BoundingBox.CreateFromSphere(ref sphere, out result);
        }

        // I/F
        public void UpdateOcclusion() { }

        // I/F
        public void Draw(Texture2D shadowMap)
        {
            // 空の色。
            Effect.SkyColor = SceneSettings.SkyColor;

            // 太陽の情報。
            if (SunVisible)
            {
                Effect.SunDiffuseColor = SceneSettings.SunlightDiffuseColor;
                Effect.SunDirection = SceneSettings.SunDirection;
                Effect.SunThreshold = sunThreshold;
            }
            Effect.SunVisible = SunVisible;

            // FarPlaneDistance の先に球が現れるように球の十分な半径を決定。
            // 描画は FarPlaneDistance が最遠ではない点に注意。
            // 球メッシュは粗い頂点データである点にも注意。

            var camera = Context.ActiveCamera;
            var projection = camera.Projection;
            
            // 元となる球メッシュは半径 0.5f であるため二倍しておく。
            var sphereScale = CalculateRadius();

            // FarPlaneDistance を越える位置に描画するため、
            // 専用の FarPlaneDistance でプロジェクション行列を作成して利用。
            // この FarPlaneDistance は球の半径より少し大きければ良い。
            var specificFar = sphereScale + 1;
            Matrix specificProjection;
            Matrix.CreatePerspectiveFieldOfView(
                projection.Fov, projection.AspectRatio, projection.NearPlaneDistance, specificFar, out specificProjection);

            // 球の座標は視点座標。
            var eyePosition = camera.Position;

            Matrix translation;
            Matrix.CreateTranslation(ref eyePosition, out translation);

            Matrix scale;
            Matrix.CreateScale(sphereScale, out scale);

            Matrix world;
            Matrix.Multiply(ref scale, ref translation, out world);

            Matrix worldView;
            Matrix.Multiply(ref world, ref camera.View.Matrix, out worldView);
            Matrix worldViewProjection;
            Matrix.Multiply(ref worldView, ref specificProjection, out worldViewProjection);

            Effect.WorldViewProjection = worldViewProjection;

            GraphicsDevice.SetVertexBuffer(sphereMesh.VertexBuffer);
            GraphicsDevice.Indices = sphereMesh.IndexBuffer;

            Effect.EnableDefaultTechnique();
            Effect.Apply();

            GraphicsDevice.DrawIndexedPrimitives(sphereMesh.PrimitiveType, 0, 0, sphereMesh.NumVertices, 0, sphereMesh.PrimitiveCount);
        }

        float CalculateRadius()
        {
            return Context.ActiveCamera.Projection.FarPlaneDistance * 10;
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri:" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + "]";
        }

        #endregion
    }
}
