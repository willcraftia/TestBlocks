#region Using

using System;
using System.Collections.Generic;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class CommandPool
    {
        Dictionary<Type, ConcurrentPool<PooledCommand>> poolDictionary = new Dictionary<Type, ConcurrentPool<PooledCommand>>();

        public void Register(Type type, Func<PooledCommand> createFunction)
        {
            poolDictionary[type] = new ConcurrentPool<PooledCommand>(createFunction);
        }

        public void Deregister(Type type)
        {
            poolDictionary.Remove(type);
        }

        public PooledCommand Borrow(Type type)
        {
            var command = poolDictionary[type].Borrow();
            command.Pool = this;
            return command;
        }

        public void Return(PooledCommand command)
        {
            poolDictionary[command.GetType()].Return(command);
        }
    }
}
