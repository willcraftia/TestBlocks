#region Using

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Collections
{
    public sealed class LruDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        #region Node

        class Node
        {
            public TKey Key { get; private set; }

            public TValue Value { get; private set; }

            public Node Previous { get; set; }

            public Node Next { get; set; }

            public Node(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }

        #endregion

        #region Enumerator

        struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            LruDictionary<TKey, TValue> dictionary;

            Node node;

            Node current;

            // I/F
            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                }
            }

            public Enumerator(LruDictionary<TKey, TValue> dictionary)
            {
                this.dictionary = dictionary;
                node = dictionary.head;
                current = null;
            }

            // I/F
            public void Dispose() { }

            // I/F
            object IEnumerator.Current
            {
                get
                {
                    return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                }
            }

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
                node = dictionary.head;
                current = null;
            }
        }

        #endregion

        #region ValueCollection

        class ValueCollection : ICollection<TValue>
        {
            #region Enumerator

            struct Enumerator : IEnumerator<TValue>
            {
                ValueCollection valueCollection;

                Node node;

                Node current;

                // I/F
                public TValue Current
                {
                    get { return current.Value; }
                }

                // I/F
                object IEnumerator.Current
                {
                    get { return current.Value; }
                }

                public Enumerator(ValueCollection valueCollection)
                {
                    this.valueCollection = valueCollection;
                    node = valueCollection.dictionary.head;
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
                    node = valueCollection.dictionary.head;
                    current = null;
                }
            }

            #endregion

            LruDictionary<TKey, TValue> dictionary;

            // I/F
            public int Count
            {
                get { return dictionary.Count; }
            }

            // I/F
            public bool IsReadOnly
            {
                get { return true; }
            }

            public ValueCollection(LruDictionary<TKey, TValue> dictionary)
            {
                this.dictionary = dictionary;
            }

            // I/F
            public void Add(TValue item) { throw new NotSupportedException(); }

            // I/F
            public void Clear() { throw new NotSupportedException(); }

            // I/F
            public bool Contains(TValue item)
            {
                if (dictionary.head == null) return false;

                for (var node = dictionary.head; node.Next != null; node = node.Next)
                {
                    if (node.Value.Equals(item)) return true;
                }

                return false;
            }

            // I/F
            public void CopyTo(TValue[] array, int arrayIndex)
            {
                if (array == null) throw new ArgumentNullException("array");
                if (arrayIndex < 0 || array.Length <= arrayIndex || array.Length - arrayIndex < Count)
                    throw new ArgumentOutOfRangeException("arrayIndex");

                foreach (var item in this)
                    array[arrayIndex++] = item;
            }

            // I/F
            public bool Remove(TValue item) { throw new NotSupportedException(); }

            // I/F
            public IEnumerator<TValue> GetEnumerator()
            {
                return new Enumerator(this);
            }

            // I/F
            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(this);
            }
        }

        #endregion

        Dictionary<TKey, Node> dictionary;

        Node head;

        Node tail;

        ValueCollection values;

        // I/F
        public ICollection<TKey> Keys
        {
            get { return dictionary.Keys; }
        }

        // I/F
        public ICollection<TValue> Values
        {
            get
            {
                if (values == null) values = new ValueCollection(this);
                return values;
            }
        }

        // I/F
        public TValue this[TKey key]
        {
            get
            {
                if (key == null) throw new ArgumentNullException("key");

                Node node;
                if (!dictionary.TryGetValue(key, out node))
                    throw new KeyNotFoundException("Invalid key: " + key);

                return node.Value;
            }
            set
            {
                Add(key, value);
            }
        }

        // I/F
        public int Count
        {
            get { return dictionary.Count; }
        }

        // I/F
        public bool IsReadOnly
        {
            get { return false; }
        }

        public int Capacity { get; private set; }

        public LruDictionary(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException("capacity");

            Capacity = capacity;
            dictionary = new Dictionary<TKey, Node>(capacity);
        }

        void Add(Node node)
        {
            node.Next = null;
            node.Previous = tail;

            if (tail != null) tail.Next = node;
            tail = node;

            if (head == null) head = node;
        }

        void Remove(Node node)
        {
            if (node.Previous != null)
            {
                node.Previous.Next = node.Next;
                node.Previous = null;
            }
            else
            {
                head = node.Next;
            }

            if (node.Next != null)
            {
                node.Next.Previous = node.Previous;
                node.Next = null;
            }
            else
            {
                tail = node.Previous;
            }
        }

        // I/F
        public void Add(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (value == null) throw new ArgumentNullException("value");

            if (Capacity <= Count) Remove(head.Key);

            Node node;
            if (dictionary.TryGetValue(key, out node))
            {
                Remove(node);
            }
            else
            {
                node = new Node(key, value);
                dictionary[node.Key] = node;
            }

            Add(node);
        }

        // I/F
        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        // I/F
        public bool Remove(TKey key)
        {
            Node node;
            if (dictionary.TryGetValue(key, out node))
            {
                dictionary.Remove(key);
                Remove(node);
                return true;
            }

            return false;
        }

        // I/F
        public bool TryGetValue(TKey key, out TValue value)
        {
            Node node;
            if (dictionary.TryGetValue(key, out node))
            {
                Remove(node);
                Add(node);
                value = node.Value;
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        // I/F
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        // I/F
        public void Clear()
        {
            dictionary.Clear();
            head = null;
            tail = null;
        }

        // I/F
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return dictionary.ContainsKey(item.Key);
        }

        // I/F
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (arrayIndex < 0 || array.Length <= arrayIndex || array.Length - arrayIndex < Count)
                throw new ArgumentOutOfRangeException("arrayIndex");

            foreach (var item in this)
                array[arrayIndex++] = item;
        }

        // I/F
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        // I/F
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
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
