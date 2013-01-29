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

        public abstract void Do();

        public abstract void Undo();

        public abstract void Release();
    }
}
