
#region Using

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Collections
{
    public class ListBase<T> : IList<T>
    {
        // List のデフォルト値と同じ。
        const int defaultCapacity = 4;

        List<T> list;

        public int Capacity
        {
            get { return list.Capacity; }
            set { list.Capacity = value; }
        }

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

        // I/F
        public T this[int index]
        {
            get { return list[index]; }
            set { SetOverride(index, value); }
        }

        public ListBase()
            : this(defaultCapacity)
        {
        }

        public ListBase(int capacity)
        {
            list = new List<T>(capacity);
        }

        // I/F
        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        // I/F
        public void Insert(int index, T item)
        {
            InsertOverride(index, item);
        }

        // I/F
        public void RemoveAt(int index)
        {
            RemoveAtOverride(index);
        }

        // I/F
        public void Add(T item)
        {
            var index = list.Count;
            InsertOverride(index, item);
        }

        // I/F
        public void Clear()
        {
            ClearOverride();
        }

        // I/F
        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        // I/F
        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        // I/F
        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index < 0) return false;

            RemoveAtOverride(index);
            return true;
        }

        // I/F
        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }
        // I/F
        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        protected virtual void ClearOverride()
        {
            list.Clear();
        }

        protected virtual void InsertOverride(int index, T item)
        {
            list.Insert(index, item);
        }

        protected virtual void SetOverride(int index, T item)
        {
            list[index] = item;
        }

        protected virtual void RemoveAtOverride(int index)
        {
            list.RemoveAt(index);
        }
    }
}
