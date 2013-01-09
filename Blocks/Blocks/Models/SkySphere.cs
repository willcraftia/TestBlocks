#region Using

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class SkySphere : SceneObject, IAsset
    {
        //
        // TODO
        //
        // 後々、空テクスチャはバイオームで管理し、
        // バイオームの切り替わりと共に様相が変化するように修正したい。
        //

        SphereMesh sphereMesh;

        bool sunVisible = true;

        // 0.999f 以上くらいでほどほどの太陽の大きさとなる。
        float sunThreshold = 0.999f;

        Vector3[] frustumCorners = new Vector3[8];

        // PreDraw メソッドで更新。
        Matrix world;

        // I/F
        public IResource Resource { get; set; }

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

        public void Update()
        {
            // シーン マネージャの可視判定で BoundingSphere/BoundingBox が用いられるため、
            // それよりも前に Update メソッドで状態を更新する必要がある。

            var camera = Context.ActiveCamera;

            //----------------------------------------------------------------
            // 原点の更新

            Position = camera.View.Position;

            //----------------------------------------------------------------
            // BoundingSphere/BoundingBox の更新

            // 視錐台の頂点で最も遠い場所を半径にする。
            // インデックス 4 から 7 の頂点のどれでも良い。
            camera.Frustum.GetCorners(frustumCorners);
            var far = Vector3.Distance(Vector3.Zero, frustumCorners[4]);

            BoundingSphere.Center = Position;
            BoundingSphere.Radius = far;
            BoundingBox.CreateFromSphere(ref BoundingSphere, out BoundingBox);
        }

        public override void PreDraw()
        {
            //----------------------------------------------------------------
            // ワールド行列の更新

            Matrix translation;
            Matrix.CreateTranslation(ref Position, out translation);

            // 元となる球メッシュは半径 0.5f であるため二倍しておく。
            Matrix scale;
            Matrix.CreateScale(BoundingSphere.Radius * 2, out scale);

            Matrix.Multiply(ref scale, ref translation, out world);

            base.PreDraw();
        }

        public override void Draw()
        {
            Debug.Assert(SceneSettings != null);

            // 空の色。
            Effect.SkyColor = SceneSettings.SkyColor;

            // 太陽の情報。
            if (SunVisible)
            {
                Effect.SunDiffuseColor = SceneSettings.Sunlight.DiffuseColor;
                Effect.SunDirection = SceneSettings.SunDirection;
                Effect.SunThreshold = sunThreshold;
            }
            Effect.SunVisible = SunVisible;

            // FarPlaneDistance の先に球が現れるように球の十分な半径を決定。
            // 描画は FarPlaneDistance が最遠ではない点に注意。
            // 球メッシュは粗い頂点データである点にも注意。

            var camera = Context.ActiveCamera;
            var projection = camera.Projection;
            
            // FarPlaneDistance を越える位置に描画するため、
            // 専用の FarPlaneDistance でプロジェクション行列を作成して利用。
            // この FarPlaneDistance は球の半径より少し大きければ良い。
            var specificFar = BoundingSphere.Radius + 1;
            Matrix specificProjection;
            Matrix.CreatePerspectiveFieldOfView(
                projection.Fov, projection.AspectRatio, projection.NearPlaneDistance, specificFar, out specificProjection);

            Matrix worldView;
            Matrix.Multiply(ref world, ref camera.View.Matrix, out worldView);
            Matrix worldViewProjection;
            Matrix.Multiply(ref worldView, ref specificProjection, out worldViewProjection);

            Effect.WorldViewProjection = worldViewProjection;

            GraphicsDevice.SetVertexBuffer(sphereMesh.VertexBuffer);
            GraphicsDevice.Indices = sphereMesh.IndexBuffer;

            Effect.Apply();

            GraphicsDevice.DrawIndexedPrimitives(sphereMesh.PrimitiveType, 0, 0, sphereMesh.NumVertices, 0, sphereMesh.PrimitiveCount);

            // スカイ スフィアはシェーダ内で CW で描画するため、念のためデフォルトへ戻しておく。
            // しかし、なるべく各シェーダ内で CullMode を明示する。
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        public override void Draw(Effect effect)
        {
            //----------------------------------------------------------------
            // エフェクト

            var effectMatrices = effect as IEffectMatrices;
            if (effectMatrices != null) effectMatrices.World = world;

            //----------------------------------------------------------------
            // 描画

            sphereMesh.Draw(effect);
        }

        public override void Draw(ShadowMap shadowMap)
        {
            // スカイ スフィアには影を投影しません。
            Draw();
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri:" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + "]";
        }

        #endregion
    }
}
