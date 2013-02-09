#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public abstract class PooledCommand : Command
    {
        internal CommandPool Pool { get; set; }

        protected PooledCommand() { }

        public override void Release()
        {
            Pool.Return(this);

            base.Release();
        }
    }
}
