#region Using

using System;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Graphics;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class FreeBrush
    {
        CommandManager commandManager;

        WorldCommandFactory commandFactory;

        SceneNode node;

        BrushMesh mesh;

        ChunkManager chunkManager;

        VectorI3 position;

        byte currentBlockIndex;

        public VectorI3 Position
        {
            get { return position; }
        }

        public byte BlockIndex { get; set; }

        public int UpdateMeshPriority { get; set; }

        public Vector3 Color { get; set; }

        public float Alpha { get; set; }

        public FreeBrush(CommandManager commandManager, WorldCommandFactory commandFactory,
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

            Color = new Vector3(1, 0, 0);
            Alpha = 0.5f;

            UpdateMeshPriority = ChunkManager.UserEditUpdateMeshPriority;
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

            const int offset = 4;
            var basePositionWorld = eyePositionWorld + eyeDirection * offset;

            position = new VectorI3
            {
                X = (int) Math.Floor(basePositionWorld.X),
                Y = (int) Math.Floor(basePositionWorld.Y),
                Z = (int) Math.Floor(basePositionWorld.Z)
            };
            currentBlockIndex = Block.EmptyIndex;

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

            node.Update(true);

            Matrix translation;
            Matrix.CreateTranslation(ref meshPositionWorld, out translation);

            Matrix scale;
            Matrix.CreateScale(1.001f, out scale);

            Matrix world;
            Matrix.Multiply(ref scale, ref translation, out world);

            mesh.Effect.DiffuseColor = Color;
            mesh.Effect.Alpha = Alpha;
            mesh.Effect.World = world;
            mesh.Effect.View = view.Matrix;
            mesh.Effect.Projection = projection.Matrix;
        }

        public void Paint()
        {
            // 既存のブロックと同じブロックを配置しようとしているならば抑制。
            if (currentBlockIndex == BlockIndex) return;

            var command = commandFactory.CreateSetBlockCommand();

            command.BlockPosition = position;
            command.BlockIndex = BlockIndex;
            command.UpdateMeshPriority = UpdateMeshPriority;

            commandManager.RequestCommand(command);
        }
    }
}
