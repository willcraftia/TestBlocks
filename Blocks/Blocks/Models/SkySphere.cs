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
    public sealed class SkySphere : IAsset
    {
        //
        // TODO
        //
        // 後々、空テクスチャはバイオームで管理し、
        // バイオームの切り替わりと共に様相が変化するように修正したい。
        //

        GraphicsDevice graphicsDevice;

        SphereMesh sphereMesh;

        // 0.999f 以上くらいでほどほどの太陽の大きさとなる。
        float sunThreshold = 0.999f;

        bool sunVisible = true;

        // I/F
        public IResource Resource { get; set; }

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

            this.graphicsDevice = graphicsDevice;

            sphereMesh = new SphereMesh(graphicsDevice);
        }

        public void Draw(View view, PerspectiveFov projection)
        {
            // 一日の時間を [0, 1] へ変換。
            // 0 が 0 時、1 が 24 時。
            var elapsed = SceneSettings.ElapsedSecondsPerDay / SceneSettings.SecondsPerDay;

            // 空の色。
            Vector3 skyColor;
            SceneSettings.ColorTable.GetColor(elapsed, out skyColor);
            Effect.SkyColor = skyColor;

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

            var sphereRaridus = projection.FarPlaneDistance * 10;
            // 元となる球メッシュは半径 0.5f であるため二倍しておく。
            var sphereScale = 2 * sphereRaridus;

            // FarPlaneDistance を越える位置に描画するため、
            // 専用の FarPlaneDistance でプロジェクション行列を作成して利用。
            // この FarPlaneDistance は球の半径より少し大きければ良い。
            var specificFar = sphereScale + 1;
            Matrix specificProjection;
            Matrix.CreatePerspectiveFieldOfView(
                projection.Fov, projection.AspectRatio, projection.NearPlaneDistance, specificFar, out specificProjection);

            // 球の座標は視点座標。
            Vector3 eyePosition;
            View.GetEyePosition(ref view.Matrix, out eyePosition);

            Matrix translation;
            Matrix.CreateTranslation(ref eyePosition, out translation);

            Matrix scale;
            Matrix.CreateScale(sphereScale, out scale);

            Matrix world;
            Matrix.Multiply(ref scale, ref translation, out world);

            Matrix worldView;
            Matrix.Multiply(ref world, ref view.Matrix, out worldView);
            Matrix worldViewProjection;
            Matrix.Multiply(ref worldView, ref specificProjection, out worldViewProjection);

            Effect.WorldViewProjection = worldViewProjection;

            graphicsDevice.SetVertexBuffer(sphereMesh.VertexBuffer);
            graphicsDevice.Indices = sphereMesh.IndexBuffer;

            Effect.EnableDefaultTechnique();
            Effect.Apply();

            graphicsDevice.DrawIndexedPrimitives(sphereMesh.PrimitiveType, 0, 0, sphereMesh.NumVertices, 0, sphereMesh.PrimitiveCount);
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri:" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + "]";
        }

        #endregion
    }
}
