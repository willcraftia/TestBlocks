#region Using

using System;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class Brush
    {
        CommandManager commandManager;

        WorldCommandFactory commandFactory;

        VectorI3 blockPosition;

        public byte BlockIndex { get; set; }

        public float Offset { get; set; }

        public Brush(CommandManager commandManager, WorldCommandFactory commandFactory)
        {
            if (commandManager == null) throw new ArgumentNullException("commandManager");
            if (commandFactory == null) throw new ArgumentNullException("commandFactory");

            this.commandManager = commandManager;
            this.commandFactory = commandFactory;
        }

        // まずはカメラを更新。
        // ブロック生成消滅の位置を決定する以外にも、
        // ブラシ描画のための描画位置の決定に必須。

        public void Update(Vector3 eyePositionWorld, Vector3 eyeDirection)
        {
            if (eyeDirection.IsZero()) throw new ArgumentException("eyeDirection must be not a zero vector.", "eyeDirection");

            eyeDirection.Normalize();

            var brushPositionWorld = eyePositionWorld + eyeDirection * Offset;

            blockPosition = new VectorI3
            {
                X = (int) Math.Floor(brushPositionWorld.X),
                Y = (int) Math.Floor(brushPositionWorld.Y),
                Z = (int) Math.Floor(brushPositionWorld.Z)
            };
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
