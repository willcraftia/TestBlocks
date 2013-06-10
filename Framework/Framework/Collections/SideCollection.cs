#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Collections
{
    public sealed class SideCollection<T> : IList<T>
    {
        #region Enumerator

        public struct Enumerator : IEnumerator<T>, System.Collections.IEnumerator
        {
            SideCollection<T> owner;

            int index;

            int version;

            T current;

            // I/F
            public T Current
            {
                get { return current; }
            }

            // I/F
            object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (index == 0 || index == owner.Count + 1) throw new IndexOutOfRangeException("index");
                    return Current;
                }
            }

            internal Enumerator(SideCollection<T> owner)
            {
                this.owner = owner;
                index = 0;
                version = owner.version;
                current = default(T);
            }

            // I/F
            public void Dispose() { }

            // I/F
            public bool MoveNext()
            {
                if (version == owner.version && index < owner.Count)
                {
                    current = owner.items[index];
                    index++;
                    return true;
                }
                return MoveNextRare();
            }

            // I/F
            public void Reset()
            {
                if (version != owner.version) throw new InvalidOperationException("Version mismatch.");

                index = 0;
                current = default(T);
            }

            bool MoveNextRare()
            {
                if (version != owner.version) throw new InvalidOperationException("Version mismatch.");

                index = owner.Count + 1;
                current = default(T);
                return false;
            }
        }

        #endregion

        T[] items = new T[Side.Count];

        int version;

        // I/F
        public T this[int index]
        {
            get { return items[index]; }
            set
            {
                items[index] = value;
                version++;
            }
        }

        // I/F
        public int Count
        {
            get { return items.Length; }
        }

        // I/F
        public bool IsReadOnly
        {
            get { return false; }
        }

        public T this[Side side]
        {
            get { return this[side.Index]; }
            set { this[side.Index] = value; }
        }

        // I/F
        public int IndexOf(T item)
        {
            return Array.IndexOf(items, item);
        }

        // I/F
        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        // I/F
        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        // I/F
        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        // I/F
        public void Clear()
        {
            Array.Clear(items, 0, items.Length);
            version++;
        }

        // I/F
        public bool Contains(T item)
        {
            if ((object) item == null)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if ((object) items[i] == null) return true;
                }
                return false;
            }
            else
            {
                EqualityComparer<T> c = EqualityComparer<T>.Default;
                for (int i = 0; i < items.Length; i++)
                {
                    if (c.Equals(items[i], item)) return true;
                }
                return false;
            }
        }

        // I/F
        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(items, 0, array, arrayIndex, items.Length);
        }

        // I/F
        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        // I/F
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        // I/F
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}
