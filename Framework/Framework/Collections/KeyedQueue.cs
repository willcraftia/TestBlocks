#region Using

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Collections
{
    public abstract class KeyedQueue<TKey, TItem> : IEnumerable<TItem>
    {
        Queue<TItem> queue;

        Dictionary<TKey, TItem> dictionary;

        public int Count
        {
            get { return queue.Count; }
        }

        public TItem this[TKey key]
        {
            get
            {
                if (key == null) throw new ArgumentNullException("key");
                return dictionary[key];
            }
        }

        protected IDictionary<TKey, TItem> Dictionary
        {
            get { return dictionary; }
        }

        protected KeyedQueue(int capacity)
        {
            queue = new Queue<TItem>(capacity);
            dictionary = new Dictionary<TKey, TItem>(capacity);
        }

        // I/F
        public IEnumerator<TItem> GetEnumerator()
        {
            return queue.GetEnumerator();
        }
        // I/F
        IEnumerator IEnumerable.GetEnumerator()
        {
            return queue.GetEnumerator();
        }

        public void Enqueue(TItem item)
        {
            if (item == null) throw new ArgumentNullException("item");

            var key = GetKey(item);
            dictionary[key] = item;
            queue.Enqueue(item);
        }

        public TItem Dequeue()
        {
            var item = queue.Dequeue();
            var key = GetKey(item);
            dictionary.Remove(key);
            return item;
        }

        public TItem Peek()
        {
            return queue.Peek();
        }

        public bool Contains(TKey key)
        {
            if (key == null) throw new ArgumentNullException("key");

            return dictionary.ContainsKey(key);
        }

        public void Clear()
        {
            queue.Clear();
            dictionary.Clear();
        }

        protected abstract TKey GetKeyForItem(TItem item);

        protected TKey GetKey(TItem item)
        {
            var key = GetKeyForItem(item);
            if (key == null) throw new InvalidOperationException("Key is null.");

            return key;
        }
    }
}
