#region Using

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Collections
{
    public sealed class LruCache<T> : ICollection<T>
    {
        #region LruCacheEnumerator

        struct LruCacheEnumerator : IEnumerator<T>
        {
            LruCache<T> cache;

            LinkedListNode<T> current;

            // I/F
            public T Current
            {
                get { return current.Value; }
            }

            public LruCacheEnumerator(LruCache<T> cache)
            {
                this.cache = cache;
                current = cache.list.First;
            }

            // I/F
            public void Dispose() { }

            // I/F
            object IEnumerator.Current
            {
                get { return current.Value; }
            }

            // I/F
            public bool MoveNext()
            {
                current = current.Next;
                return current != null;
            }

            // I/F
            public void Reset()
            {
                current = cache.list.First;
            }
        }

        #endregion

        const int defaultCapacity = 100;

        LinkedList<T> list = new LinkedList<T>();

        Dictionary<T, LinkedListNode<T>> index;

        // I/F
        public int Count
        {
            get { return list.Count; }
        }

        // I/F
        public bool IsReadOnly
        {
            get { return false; }
        }

        public int Capacity { get; private set; }

        public LruCache()
            : this(defaultCapacity)
        {
        }

        public LruCache(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity");

            Capacity = capacity;
            index = new Dictionary<T, LinkedListNode<T>>(capacity);
        }

        public bool Touch(T item)
        {
            LinkedListNode<T> node;
            if (index.TryGetValue(item, out node))
            {
                list.Remove(node);
                list.AddLast(node);
                return true;
            }

            return false;
        }

        // I/F
        public void Add(T item)
        {
            if (Touch(item)) return;

            if (Capacity <= Count && Capacity != 0)
            {
                var oldest = list.First.Value;
                Remove(oldest);
            }

            index[item] = list.AddLast(item);
        }

        // I/F
        public void Clear()
        {
            list.Clear();
            index.Clear();
        }

        // I/F
        public bool Contains(T item)
        {
            return Touch(item);
        }

        // I/F
        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var item in this)
                array[arrayIndex++] = item;
        }

        // I/F
        public bool Remove(T item)
        {
            LinkedListNode<T> node;
            if (!index.TryGetValue(item, out node)) return false;

            list.Remove(node);
            index.Remove(item);
            return true;
        }

        // I/F
        public IEnumerator<T> GetEnumerator()
        {
            return new LruCacheEnumerator(this);
        }

        // I/F
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new LruCacheEnumerator(this);
        }
    }
}
