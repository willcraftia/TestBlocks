#region Using

using System;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class FreeBrush : Brush
    {
        BrushMesh brushMesh;

        public FreeBrush(BrushManager manager, SceneNode node)
            : base(manager, node)
        {
            var mesh = manager.LoadAsset<Mesh>("title:Resources/Meshes/Cube.json");

            brushMesh = new BrushMesh("BrushMesh", manager.GraphicsDevice, mesh);
            brushMesh.Color = new Vector3(1, 0, 0);
            brushMesh.Alpha = 0.5f;
            Node.Objects.Add(brushMesh);

            // 常にペイント可能。
            CanPaint = true;
        }

        // まずはカメラを更新。
        // ブロック生成消滅の位置を決定する以外にも、
        // ブラシ描画のための描画位置の決定に必須。

        public override void Update(ICamera camera)
        {
            var eyePositionWorld = camera.View.Position;
            var eyeDirection = camera.View.Direction;
            eyeDirection.Normalize();

            const int offset = 4;
            var basePositionWorld = eyePositionWorld + eyeDirection * offset;

            Position = new VectorI3
            {
                X = (int) Math.Floor(basePositionWorld.X),
                Y = (int) Math.Floor(basePositionWorld.Y),
                Z = (int) Math.Floor(basePositionWorld.Z)
            };

            // 自由ブラシは、その位置がペイント位置であり消去位置。
            PaintPosition = Position;
            ErasePosition = Position;

            UpdateMesh();
            Node.Update(true);
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

            brushMesh.VisibleAllFaces = true;
        }
    }
}
