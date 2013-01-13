#region Using

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Collections
{
    public sealed class Pool<T> where T : class
    {
        // 0 means the infinite capacity.
        public const int DefaultMaxCapacity = 0;

        Func<T> createFunction;

        Queue<T> objects = new Queue<T>();

        public int Count
        {
            get { return objects.Count; }
        }

        public int InitialCapacity { get; private set; }

        public int MaxCapacity { get; set; }

        public int TotalObjectCount { get; private set; }

        public Pool(Func<T> createFunction)
        {
            this.createFunction = createFunction;
            MaxCapacity = DefaultMaxCapacity;
        }

        public void Prepare(int initialCapacity)
        {
            if (initialCapacity < 0 || (MaxCapacity != 0 && MaxCapacity < initialCapacity))
                throw new ArgumentOutOfRangeException("initialCapacity");

            InitialCapacity = initialCapacity;

            for (int i = 0; i < initialCapacity; i++)
                objects.Enqueue(CreateObject());
        }

        public T Borrow()
        {
            while (0 < MaxCapacity && MaxCapacity < TotalObjectCount && 0 < objects.Count)
                DisposeObject(objects.Dequeue());

            if (0 < MaxCapacity && MaxCapacity <= TotalObjectCount && objects.Count == 0)
                return null;

            if (0 < objects.Count)
                return objects.Dequeue();

            return CreateObject();
        }

        public void Return(T obj)
        {
            if (MaxCapacity == 0 || TotalObjectCount <= MaxCapacity)
            {
                objects.Enqueue(obj);
            }
            else
            {
                DisposeObject(obj);
            }
        }

        public void Clear()
        {
            foreach (var obj in objects) DisposeObject(obj);

            objects.Clear();
        }

        T CreateObject()
        {
            if (0 < MaxCapacity && MaxCapacity < TotalObjectCount)
                return null;

            TotalObjectCount++;
            return createFunction();
        }

        void DisposeObject(T obj)
        {
            var disposable = obj as IDisposable;
            if (disposable != null) disposable.Dispose();

            TotalObjectCount--;
        }
    }
}
