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
        // メモ
        //
        // スカイ スフィアの描画では、位置、BoundingBox、BoundingSphere を用いないため、
        // それらに適切な値を設定することはなく、また、それらの値に意味はない。
        //

        SphereMesh sphereMesh;

        bool sunVisible = true;

        float sunThreshold = 0.999f;

        Vector3[] frustumCorners = new Vector3[8];

        // I/F
        public IResource Resource { get; set; }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public SkySphereEffect Effect { get; set; }

        /// <summary>
        /// 球面に描画する太陽の範囲を定める閾値を取得または設定します。
        /// 0.999f 以上くらいでほどほどの太陽の大きさです。
        /// </summary>
        public float SunThreshold
        {
            get { return sunThreshold; }
            set { sunThreshold = value; }
        }

        /// <summary>
        /// 球面に太陽を描画するか否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (球面に太陽を描画する場合)、false (それ以外の場合)。
        /// </value>
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

        public override void Draw()
        {
            Debug.Assert(SceneSettings != null);

            // 空の色。
            Effect.SkyColor = SceneSettings.CurrentSkyColor;

            // 太陽の情報。
            if (SunVisible)
            {
                // 太陽は夜間も描画し続ける。
                Effect.SunDiffuseColor = SceneSettings.Sunlight.DiffuseColor;
                Effect.SunDirection = SceneSettings.SunDirection;
                Effect.SunThreshold = sunThreshold;
            }
            Effect.SunVisible = SunVisible;

            var camera = Context.ActiveCamera;

            //
            // メモ
            //
            // XNA サンプル『生成されたジオメトリ』を参照。
            //
            
            var view = camera.View.Matrix;

            // スカイ スフィアは視点位置の影響を受けない。
            view.Translation = Vector3.Zero;

            var projection = camera.Projection.Matrix;

            // z = w を強制してファー クリップ面に描画。
            projection.M13 = projection.M14;
            projection.M23 = projection.M24;
            projection.M33 = projection.M34;
            projection.M43 = projection.M44;

            Matrix viewProjection;
            Matrix.Multiply(ref view, ref projection, out viewProjection);

            Effect.ViewProjection = viewProjection;

            GraphicsDevice.SetVertexBuffer(sphereMesh.VertexBuffer);
            GraphicsDevice.Indices = sphereMesh.IndexBuffer;

            Effect.Apply();

            // 深度は読み取り専用。
            // スカイ スフィアは最後に描画する前提。ただし、レンズ フレアの前。
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            GraphicsDevice.DrawIndexedPrimitives(sphereMesh.PrimitiveType, 0, 0, sphereMesh.NumVertices, 0, sphereMesh.PrimitiveCount);

            // デフォルトへ戻す。
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        public override void Draw(Effect effect)
        {
            // スカイ スフィアは特殊なエフェクトには対応しない (その必要もない)。
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri:" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + "]";
        }

        #endregion
    }
}
