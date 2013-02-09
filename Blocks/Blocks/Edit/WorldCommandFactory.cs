#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class WorldCommandFactory
    {
        WorldManager worldManager;

        CommandPool pool = new CommandPool();

        public WorldCommandFactory(WorldManager worldManager)
        {
            if (worldManager == null) throw new ArgumentNullException("worldManager");

            this.worldManager = worldManager;

            pool.Register(typeof(SetBlockCommand), () => { return new SetBlockCommand(); });
        }

        public SetBlockCommand CreateSetBlockCommand()
        {
            return Create(typeof(SetBlockCommand)) as SetBlockCommand;
        }

        WorldCommand Create(Type type)
        {
            var command = pool.Borrow(type) as WorldCommand;
            command.WorldManager = worldManager;
            return command;
        }
    }
}
