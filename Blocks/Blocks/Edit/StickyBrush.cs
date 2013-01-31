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
    public sealed class StickyBrush
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

        CommandManager commandManager;

        WorldCommandFactory commandFactory;

        SceneNode node;

        BrushMesh mesh;

        ChunkManager chunkManager;

        VectorI3 position;

        VectorI3 paintPosition;

        CubicSide visibleFace;

        byte currentBlockIndex;

        public VectorI3 Position
        {
            get { return position; }
        }

        public VectorI3 PaintPosition
        {
            get { return paintPosition; }
        }

        public byte BlockIndex { get; set; }

        public int UpdateMeshPriority { get; set; }

        public bool CanPaint { get; set; }

        public StickyBrush(CommandManager commandManager, WorldCommandFactory commandFactory,
            SceneNode node, BrushMesh mesh, ChunkManager chunkManager)
        {
            if (commandManager == null) throw new ArgumentNullException("commandManager");
            if (commandFactory == null) throw new ArgumentNullException("commandFactory");
            if (node == null) throw new ArgumentNullException("node");
            if (mesh == null) throw new ArgumentNullException("mesh");
            if (chunkManager == null) throw new ArgumentNullException("chunkManager");

            this.commandManager = commandManager;
            this.commandFactory = commandFactory;
            this.node = node;
            this.mesh = mesh;
            this.chunkManager = chunkManager;

            if (!node.Objects.Contains(mesh)) node.Objects.Add(mesh);

            UpdateMeshPriority = ChunkManager.UserEditUpdateMeshPriority;

            triangleInfos = new TriangleInfo[2 * 6];

            int i = 0;
            foreach (var side in CubicSide.Items)
            {
                var normal = side.Direction.ToVector3();

                var side1 = new Vector3(normal.Y, normal.Z, normal.X);
                var side2 = Vector3.Cross(normal, side1);

                var v0 = (normal - side1 - side2) * 0.5f;
                var v1 = (normal - side1 + side2) * 0.5f;
                var v2 = (normal + side1 + side2) * 0.5f;
                var v3 = (normal + side1 - side2) * 0.5f;

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

        // まずはカメラを更新。
        // ブロック生成消滅の位置を決定する以外にも、
        // ブラシ描画のための描画位置の決定に必須。

        public void Update(View view, Projection projection)
        {
            CanPaint = false;

            var eyePositionWorld = view.Position;
            var eyeDirection = view.Direction;

            if (eyeDirection.IsZero()) throw new ArgumentException("eyeDirection must be not a zero vector.", "eyeDirection");

            eyeDirection.Normalize();

            var ray = new Ray(eyePositionWorld, eyeDirection);

            // グリッドに沿っていない視点方向を考慮しての float によるオフセット。
            var prevTestPosition = new VectorI3();
            for (float offset = 0.5f; offset < 10; offset += 0.2f)
            {
                var basePositionWorld = eyePositionWorld + eyeDirection * offset;

                var testPosition = new VectorI3
                {
                    X = (int) Math.Floor(basePositionWorld.X),
                    Y = (int) Math.Floor(basePositionWorld.Y),
                    Z = (int) Math.Floor(basePositionWorld.Z)
                };

                if (prevTestPosition == testPosition) continue;

                var chunk = chunkManager.GetChunkByBlockPosition(testPosition);
                if (chunk == null) continue;

                var relativePosition = chunk.GetRelativeBlockPosition(testPosition);

                var blockIndex = chunk[relativePosition];
                if (blockIndex != Block.EmptyIndex)
                {
                    if (ResolvePaintPosition(ref ray, ref testPosition))
                    {
                        position = testPosition;
                        CanPaint = true;
                        break;
                    }
                }

                prevTestPosition = testPosition;
            }

            mesh.Visible = CanPaint;

            if (!CanPaint) return;

            UpdateMesh();
            node.Update(true);
        }

        bool ResolvePaintPosition(ref Ray ray, ref VectorI3 brushPosition)
        {
            var brushPositionWorld = brushPosition.ToVector3();
            brushPositionWorld.X += 0.5f;
            brushPositionWorld.Y += 0.5f;
            brushPositionWorld.Z += 0.5f;

            Matrix world;
            Matrix.CreateTranslation(ref brushPositionWorld, out world);

            for (int i = 0; i < triangleInfos.Length; i++)
            {
                Triangle triangleWorld;
                triangleInfos[i].CreateTriangleWorld(ref world, out triangleWorld);

                float? distance;
                triangleWorld.Intersects(ref ray, out distance);

                if (distance != null)
                {
                    var testPosition = brushPosition + triangleInfos[i].Side.Direction;
                    var chunk = chunkManager.GetChunkByBlockPosition(testPosition);
                    if (chunk == null) continue;

                    var relativePosition = chunk.GetRelativeBlockPosition(testPosition);
                    var blockIndex = chunk[relativePosition];
                    if (blockIndex == Block.EmptyIndex)
                    {
                        paintPosition = testPosition;
                        currentBlockIndex = blockIndex;
                        visibleFace = triangleInfos[i].Side;
                        return true;
                    }
                }
            }

            return false;
        }

        public void Paint()
        {
            if (!CanPaint) return;

            // 既存のブロックと同じブロックを配置しようとしているならば抑制。
            if (currentBlockIndex == BlockIndex) return;

            var command = commandFactory.CreateSetBlockCommand();

            command.BlockPosition = paintPosition;
            command.BlockIndex = BlockIndex;
            command.UpdateMeshPriority = UpdateMeshPriority;

            commandManager.RequestCommand(command);
        }

        void UpdateMesh()
        {
            var meshPositionWorld = new Vector3
            {
                X = position.X + 0.5f,
                Y = position.Y + 0.5f,
                Z = position.Z + 0.5f
            };

            mesh.PositionWorld = meshPositionWorld;
            mesh.BoxWorld.Min = meshPositionWorld - new Vector3(0.5f);
            mesh.BoxWorld.Max = meshPositionWorld + new Vector3(0.5f);

            BoundingSphere.CreateFromBoundingBox(ref mesh.BoxWorld, out mesh.SphereWorld);

            mesh.VisibleAllFaces = false;
            mesh.VisibleFace = visibleFace;
        }
    }
}
