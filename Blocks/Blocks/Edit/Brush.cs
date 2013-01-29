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

            var brushPositionWorld = eyePositionWorld + eyeDirection * Offset;

            blockPosition = new VectorI3
            {
                X = (int) Math.Floor(brushPositionWorld.X),
                Y = (int) Math.Floor(brushPositionWorld.Y),
                Z = (int) Math.Floor(brushPositionWorld.Z)
            };

            Matrix world;
            Matrix.CreateTranslation(ref brushPositionWorld, out world);

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
