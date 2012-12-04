﻿#region Using

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework
{
    public sealed class Pool<T> : IEnumerable<T> where T : class
    {
        public const int DefaultInitialCapacity = 0;

        // 0 means the infinite capacity.
        public const int DefaultMaxCapacity = 0;

        Func<T> createFunction;

        Stack<T> objects = new Stack<T>();

        public int InitialCapacity { get; private set; }

        public int MaxCapacity { get; set; }

        public int TotalObjectCount { get; private set; }

        public int FreeObjectCount
        {
            get { return objects.Count; }
        }

        public Pool(Func<T> createFunction)
            : this(createFunction, DefaultInitialCapacity)
        {
        }

        public Pool(Func<T> createFunction, int initialCapacity)
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
            while (0 < MaxCapacity && MaxCapacity < TotalObjectCount && 0 < objects.Count)
                objects.Pop();

            if (0 < objects.Count)
                return objects.Pop();

            return CreateObject();
        }

        public void Return(T obj)
        {
            if (TotalObjectCount <= MaxCapacity)
                objects.Push(obj);
        }

        public void Clear()
        {
            objects.Clear();
        }

        // I/F
        public IEnumerator<T> GetEnumerator()
        {
            return objects.GetEnumerator();
        }

        // I/F
        IEnumerator IEnumerable.GetEnumerator()
        {
            return objects.GetEnumerator();
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
