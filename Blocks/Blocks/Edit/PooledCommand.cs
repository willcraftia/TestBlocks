#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public abstract class PooledCommand : Command
    {
        internal CommandPool Pool { get; set; }

        protected PooledCommand() { }

        public sealed override void Release()
        {
            ReleaseOverride();

            Pool.Return(this);

            base.Release();
        }

        protected virtual void ReleaseOverride() { }
    }
}
