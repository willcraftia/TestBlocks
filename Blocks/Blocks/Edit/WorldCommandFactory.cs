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

            pool.Register(() => { return new SetBlockCommand(); });
        }

        public T Create<T>() where T : WorldCommand
        {
            var command = pool.Borrow<T>();
            command.WorldManager = worldManager;
            return command;
        }
    }
}
