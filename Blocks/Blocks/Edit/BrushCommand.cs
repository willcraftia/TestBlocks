#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public abstract class BrushCommand : PooledCommand
    {
        protected internal BrushManager BrushManager { get; set; }

        protected BrushCommand() { }
    }
}
