#region Using

using System;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class Brush
    {
        CommandManager commandManager;

        WorldCommandFactory commandFactory;

        SceneNode node;

        BrushMesh mesh;

        VectorI3 blockPosition;

        public byte BlockIndex { get; set; }

        public float Offset { get; set; }

        public Vector3 Color { get; set; }

        public float Alpha { get; set; }

        public Brush(CommandManager commandManager, WorldCommandFactory commandFactory, SceneNode node, BrushMesh mesh)
        {
            if (commandManager == null) throw new ArgumentNullException("commandManager");
            if (commandFactory == null) throw new ArgumentNullException("commandFactory");
            if (node == null) throw new ArgumentNullException("node");
            if (mesh == null) throw new ArgumentNullException("mesh");

            this.commandManager = commandManager;
            this.commandFactory = commandFactory;
            this.node = node;
            this.mesh = mesh;

            if (!node.Objects.Contains(mesh)) node.Objects.Add(mesh);

            Offset = 3;
            Color = Vector3.One;
            Alpha = 0.2f;
        }

        // まずはカメラを更新。
        // ブロック生成消滅の位置を決定する以外にも、
        // ブラシ描画のための描画位置の決定に必須。

        public void Update(View view, Projection projection)
        {
            var eyePositionWorld = view.Position;
            var eyeDirection = view.Direction;

            if (eyeDirection.IsZero()) throw new ArgumentException("eyeDirection must be not a zero vector.", "eyeDirection");

            eyeDirection.Normalize();

            var basePositionWorld = eyePositionWorld + eyeDirection * Offset;

            blockPosition = new VectorI3
            {
                X = (int) Math.Floor(basePositionWorld.X),
                Y = (int) Math.Floor(basePositionWorld.Y),
                Z = (int) Math.Floor(basePositionWorld.Z)
            };

            var meshPositionWorld = new Vector3
            {
                X = blockPosition.X + 0.5f,
                Y = blockPosition.Y + 0.5f,
                Z = blockPosition.Z + 0.5f
            };

            mesh.PositionWorld = meshPositionWorld;
            mesh.BoxWorld.Min = meshPositionWorld - new Vector3(0.5f);
            mesh.BoxWorld.Max = meshPositionWorld + new Vector3(0.5f);

            BoundingSphere.CreateFromBoundingBox(ref mesh.BoxWorld, out mesh.SphereWorld);

            node.Update(true);

            Matrix world;
            Matrix.CreateTranslation(ref meshPositionWorld, out world);

            mesh.Effect.DiffuseColor = Color;
            mesh.Effect.Alpha = Alpha;
            mesh.Effect.World = world;
            mesh.Effect.View = view.Matrix;
            mesh.Effect.Projection = projection.Matrix;
        }

        public void Paint()
        {
            var command = commandFactory.CreateSetBlockCommand();

            command.BlockPosition = blockPosition;
            command.BlockIndex = BlockIndex;

            commandManager.RequestCommand(command);
        }

        public void Erase()
        {
            var command = commandFactory.CreateSetBlockCommand();

            command.BlockPosition = blockPosition;
            command.BlockIndex = Block.EmptyIndex;

            commandManager.RequestCommand(command);
        }
    }
}
