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

        public void Register<T>(Func<T> createFunction) where T : PooledCommand
        {
            poolDictionary[typeof(T)] = new ConcurrentPool<PooledCommand>(createFunction);
        }

        public void Deregister<T>() where T : PooledCommand
        {
            poolDictionary.Remove(typeof(T));
        }

        public T Borrow<T>() where T : PooledCommand
        {
            var command = poolDictionary[typeof(T)].Borrow() as T;
            command.Pool = this;
            return command;
        }

        public void Return<T>(T command) where T : PooledCommand
        {
            poolDictionary[typeof(T)].Return(command);
        }
    }
}
