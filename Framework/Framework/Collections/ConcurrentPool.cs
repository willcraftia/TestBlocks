#region Using

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Collections
{
    public sealed class ConcurrentPool<T> where T : class
    {
        public const int DefaultInitialCapacity = 0;

        // 0 means the infinite capacity.
        public const int DefaultMaxCapacity = 0;

        Func<T> createFunction;

        Stack<T> objects = new Stack<T>();

        public int Count
        {
            get
            {
                lock (objects)
                {
                    return objects.Count;
                }
            }
        }

        public int InitialCapacity { get; private set; }

        public int MaxCapacity { get; set; }

        public int TotalObjectCount { get; private set; }

        public ConcurrentPool(Func<T> createFunction)
            : this(createFunction, DefaultInitialCapacity)
        {
        }

        public ConcurrentPool(Func<T> createFunction, int initialCapacity)
        {
            if (initialCapacity < 0)
                throw new ArgumentOutOfRangeException("initialCapacity");

            this.createFunction = createFunction;
            InitialCapacity = initialCapacity;
            MaxCapacity = DefaultMaxCapacity;

            for (int i = 0; i < initialCapacity; i++)
                objects.Push(CreateObject());
        }

        public T Borrow()
        {
            lock (objects)
            {
                while (0 < MaxCapacity && MaxCapacity < TotalObjectCount && 0 < objects.Count)
                    objects.Pop();

                if (0 < objects.Count)
                    return objects.Pop();

                return CreateObject();
            }
        }

        public void Return(T obj)
        {
            lock (objects)
            {
                if (TotalObjectCount <= MaxCapacity)
                    objects.Push(obj);
            }
        }

        public void Clear()
        {
            lock (objects)
            {
                objects.Clear();
            }
        }

        T CreateObject()
        {
            if (0 < MaxCapacity && MaxCapacity < TotalObjectCount)
                return null;

            TotalObjectCount++;
            return createFunction();
        }
    }
}
