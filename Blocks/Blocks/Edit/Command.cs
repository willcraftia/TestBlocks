#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public abstract class Command
    {
        internal readonly LinkedListNode<Command> Node;

        protected Command()
        {
            Node = new LinkedListNode<Command>(this);
        }

        public abstract bool Do();

        public virtual void Undo() { }

        public virtual void Release() { }
    }
}
