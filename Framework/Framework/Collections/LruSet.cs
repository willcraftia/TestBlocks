#region Using

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Collections
{
    public sealed class LruSet<T> : ICollection<T>
    {
        #region Enumerator

        struct Enumerator : IEnumerator<T>
        {
            LruSet<T> collection;

            LinkedListNode<T> node;

            LinkedListNode<T> current;

            // I/F
            public T Current
            {
                get { return current.Value; }
            }

            // I/F
            object IEnumerator.Current
            {
                get { return current.Value; }
            }

            public Enumerator(LruSet<T> collection)
            {
                this.collection = collection;
                node = collection.list.First;
                current = null;
            }

            // I/F
            public void Dispose() { }

            // I/F
            public bool MoveNext()
            {
                if (node == null) return false;

                current = node;
                node = node.Next;
                return true;
            }

            // I/F
            public void Reset()
            {
                node = collection.list.First;
                current = null;
            }
        }

        #endregion

        const int defaultCapacity = 100;

        LinkedList<T> list = new LinkedList<T>();

        Dictionary<T, LinkedListNode<T>> dictionary;

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

        public LruSet()
            : this(defaultCapacity)
        {
        }

        public LruSet(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException("capacity");

            Capacity = capacity;
            dictionary = new Dictionary<T, LinkedListNode<T>>(capacity);
        }

        public bool Touch(T item)
        {
            if (item == null) throw new ArgumentNullException("item");

            LinkedListNode<T> node;
            if (dictionary.TryGetValue(item, out node))
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
            if (item == null) throw new ArgumentNullException("item");

            if (Touch(item)) return;

            if (Capacity <= Count) Remove(list.First.Value);

            dictionary[item] = list.AddLast(item);
        }

        // I/F
        public void Clear()
        {
            list.Clear();
            dictionary.Clear();
        }

        // I/F
        public bool Contains(T item)
        {
            if (item == null) throw new ArgumentNullException("item");

            return Touch(item);
        }

        // I/F
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (arrayIndex < 0 || array.Length <= arrayIndex || array.Length - arrayIndex < Count)
                throw new ArgumentOutOfRangeException("arrayIndex");

            foreach (var item in this)
                array[arrayIndex++] = item;
        }

        // I/F
        public bool Remove(T item)
        {
            if (item == null) throw new ArgumentNullException("item");

            LinkedListNode<T> node;
            if (!dictionary.TryGetValue(item, out node)) return false;

            list.Remove(node);
            dictionary.Remove(item);
            return true;
        }

        // I/F
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        // I/F
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}
