#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class StickyBrush : Brush
    {
        #region TriangleInfo

        sealed class TriangleInfo
        {
            public Triangle Triangle;

            public CubicSide Side { get; private set; }

            public TriangleInfo(CubicSide side)
            {
                Side = side;
            }

            public void CreateTriangleWorld(ref Matrix world, out Triangle result)
            {
                result = new Triangle();
                Vector3.Transform(ref Triangle.V0, ref world, out result.V0);
                Vector3.Transform(ref Triangle.V1, ref world, out result.V1);
                Vector3.Transform(ref Triangle.V2, ref world, out result.V2);
            }
        }

        #endregion

        TriangleInfo[] triangleInfos;

        BrushMesh brushMesh;

        CubicSide paintSide;

        CubicSide lockedSide;

        public CubicSide PaintSide
        {
            get { return paintSide; }
        }

        public StickyBrush(BrushManager manager, SceneNode node)
            : base(manager, node)
        {
            var mesh = manager.LoadAsset<Mesh>("title:Resources/Meshes/Cube.json");

            brushMesh = new BrushMesh("BrushMesh", manager.GraphicsDevice, mesh);
            brushMesh.Color = new Vector3(1, 0, 0);
            brushMesh.Alpha = 0.5f;
            Node.Objects.Add(brushMesh);

            triangleInfos = new TriangleInfo[2 * 6];

            // 原点中心の立方体をブロックのグリッドへ移動させるための行列。
            var transform = Matrix.CreateTranslation(new Vector3(0.5f));

            int i = 0;
            for (int j = 0; j < CubicSide.Count; j++)
            {
                var side = CubicSide.Items[j];

                var normal = side.Direction.ToVector3();

                var side1 = new Vector3(normal.Y, normal.Z, normal.X);
                var side2 = Vector3.Cross(normal, side1);

                // (0,0,0) を原点に頂点を算出。
                var v0 = (normal - side1 - side2) * 0.5f;
                var v1 = (normal - side1 + side2) * 0.5f;
                var v2 = (normal + side1 + side2) * 0.5f;
                var v3 = (normal + side1 - side2) * 0.5f;

                // ブロックのグリッドに移動。
                Vector3.Transform(ref v0, ref transform, out v0);
                Vector3.Transform(ref v1, ref transform, out v1);
                Vector3.Transform(ref v2, ref transform, out v2);
                Vector3.Transform(ref v3, ref transform, out v3);

                triangleInfos[i] = new TriangleInfo(side)
                {
                    Triangle = new Triangle(v0, v1, v2)
                };

                triangleInfos[i + 1] = new TriangleInfo(side)
                {
                    Triangle = new Triangle(v0, v2, v3)
                };

                i += 2;
            }
        }

        public override void StartPaint()
        {
            lockedSide = null;

            base.StartPaint();
        }

        // まずはカメラを更新。
        // ブロック生成消滅の位置を決定する以外にも、
        // ブラシ描画のための描画位置の決定に必須。

        public override void Update(ICamera camera)
        {
            CanPaint = false;

            var eyePositionWorld = camera.View.Position;
            var eyeDirection = camera.View.Direction;
            eyeDirection.Normalize();

            var ray = new Ray(eyePositionWorld, eyeDirection);

            // グリッドに沿っていない視点方向を考慮しての float によるオフセット。
            // 約 sqrt(2) / 2 で位置を増加させつつ判定。
            var prevTestPosition = new VectorI3();
            for (float offset = 0.7f; offset < 10; offset += 0.7f)
            {
                var basePositionWorld = eyePositionWorld + eyeDirection * offset;

                var testPosition = new VectorI3
                {
                    X = (int) Math.Floor(basePositionWorld.X),
                    Y = (int) Math.Floor(basePositionWorld.Y),
                    Z = (int) Math.Floor(basePositionWorld.Z)
                };

                if (prevTestPosition == testPosition) continue;

                var blockIndex = Manager.GetBlockIndex(ref testPosition);
                if (blockIndex != null && blockIndex != Block.EmptyIndex)
                {
                    if (ResolvePaintPosition(ref ray, ref testPosition))
                    {
                        Position = testPosition;
                        CanPaint = true;
                        break;
                    }
                }

                prevTestPosition = testPosition;
            }

            // 粘着ブラシの消去位置はブラシの位置。
            ErasePosition = Position;

            // ペイント開始かつ面未固定ならば、選択された面で固定。
            if (PaintStarted && lockedSide == null)
            {
                lockedSide = paintSide;
            }

            // ペイント不能の場合は自動的にメッシュ非表示となるため、
            // メッシュの更新は不要。
            if (!CanPaint) return;

            UpdateMesh();
            Node.Update(true);
        }

        bool ResolvePaintPosition(ref Ray ray, ref VectorI3 brushPosition)
        {
            var brushPositionWorld = brushPosition.ToVector3();

            Matrix world;
            Matrix.CreateTranslation(ref brushPositionWorld, out world);

            var rayDirection = ray.Direction;
            rayDirection.Normalize();

            for (int i = 0; i < triangleInfos.Length; i++)
            {
                var side = triangleInfos[i].Side;

                // 面固定済みならば、それ以外の面を除外。
                if (PaintStarted && lockedSide != null && side != lockedSide)
                    continue;

                // 背面は判定から除外。
                if (IsBackFace(side, ref rayDirection)) continue;

                // 面と視線が交差するか否か。
                if (Intersects(ref ray, triangleInfos[i], ref world))
                {
                    var testPosition = brushPosition + side.Direction;
                    var blockIndex = Manager.GetBlockIndex(ref testPosition);
                    if (blockIndex == Block.EmptyIndex)
                    {
                        PaintPosition = testPosition;
                        paintSide = side;
                        return true;
                    }
                }
            }

            return false;
        }

        bool IsBackFace(CubicSide side, ref Vector3 eyeDirection)
        {
            var normal = side.Direction.ToVector3();

            float dot;
            Vector3.Dot(ref eyeDirection, ref normal, out dot);
            return (0 < dot);
        }

        bool Intersects(ref Ray ray, TriangleInfo triangleInfo, ref Matrix world)
        {
            Triangle triangleWorld;
            triangleInfo.CreateTriangleWorld(ref world, out triangleWorld);

            float? distance;
            triangleWorld.Intersects(ref ray, out distance);

            return distance != null;
        }

        void UpdateMesh()
        {
            var meshPositionWorld = new Vector3
            {
                X = Position.X + 0.5f,
                Y = Position.Y + 0.5f,
                Z = Position.Z + 0.5f
            };

            brushMesh.PositionWorld = meshPositionWorld;
            brushMesh.BoxWorld.Min = meshPositionWorld - new Vector3(0.5f);
            brushMesh.BoxWorld.Max = meshPositionWorld + new Vector3(0.5f);

            BoundingSphere.CreateFromBoundingBox(ref brushMesh.BoxWorld, out brushMesh.SphereWorld);

            brushMesh.VisibleAllFaces = false;
            brushMesh.VisibleFace = paintSide;
        }
    }
}
