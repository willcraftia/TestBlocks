#region Using

using System;
using Willcraftia.Xna.Blocks.Models;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public abstract class WorldCommand : PooledCommand
    {
        protected internal WorldManager WorldManager { get; set; }

        protected WorldCommand() { }
    }
}
