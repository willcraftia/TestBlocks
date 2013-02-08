#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Collections
{
    public sealed class StructList<T> where T : struct
    {
        // List のデフォルト値と同じ。
        const int defaultCapacity = 4;

        readonly T[] emptyArray = new T[0];

        T[] items;

        int size;

        /// <summary>
        /// 容量を取得または設定します。
        /// </summary>
        public int Capacity
        {
            get
            {
                return items.Length;
            }
            set
            {
                if (value < size) throw new ArgumentOutOfRangeException("value");

                if (value != items.Length)
                {
                    if (0 < value)
                    {
                        T[] newItems = new T[value];
                        if (size > 0) Array.Copy(items, 0, newItems, 0, size);
                        items = newItems;
                    }
                    else
                    {
                        items = emptyArray;
                    }
                }
            }
        }

        public int Count
        {
            get { return size; }
        }

        public T this[int index]
        {
            get
            {
                // List<T> の実装より。
                // uint キャストで負値を転じさせ、0 未満判定を削減。
                if ((uint) size <= (uint) index) throw new ArgumentOutOfRangeException("index");

                return items[index];
            }
            set
            {
                if ((uint) size <= (uint) index) throw new ArgumentOutOfRangeException("index");

                items[index] = value;
            }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <remarks>
        /// 初期状態ではサイズ 0 の配列を用います。
        /// 要素の追加が行われた場合に初期容量としてサイズ 4 の配列を確保します。
        /// </remarks>
        public StructList()
        {
            items = emptyArray;
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="capacity">初期容量。</param>
        public StructList(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity");

            items = new T[capacity];
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(items, item, 0, size);
        }

        public void Insert(int index, T item)
        {
            if ((uint) size < (uint) index) throw new ArgumentOutOfRangeException("index");

            if (size == items.Length) EnsureCapacity(size + 1);
            if (index < size)
            {
                Array.Copy(items, index, items, index + 1, size - index);
            }
            items[index] = item;
            size++;
        }

        public void RemoveAt(int index)
        {
            if ((uint) size <= (uint) index) throw new ArgumentOutOfRangeException("index");

            size--;
            if (index < size)
            {
                Array.Copy(items, index + 1, items, index, size - index);
            }
            items[size] = default(T);
        }

        public void Add(T item)
        {
            if (size == items.Length) EnsureCapacity(size + 1);
            items[size++] = item;
        }

        public void Clear()
        {
            if (0 < size)
            {
                Array.Clear(items, 0, size);
                size = 0;
            }
        }

        public bool Contains(T item)
        {
            var c = EqualityComparer<T>.Default;
            for (int i = 0; i < size; i++)
            {
                if (c.Equals(items[i], item)) return true;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(items, 0, array, arrayIndex, size);
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (0 <= index)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 要素をソートします。
        /// </summary>
        public void Sort()
        {
            Sort(0, Count, null);
        }

        /// <summary>
        /// 指定の比較オブジェクトを用いて要素をソートします。
        /// </summary>
        /// <param name="comparer">比較オブジェクト。</param>
        public void Sort(IComparer<T> comparer)
        {
            Sort(0, Count, comparer);
        }

        /// <summary>
        /// 指定の範囲にある要素をソートします。
        /// </summary>
        /// <param name="index">開始位置。</param>
        /// <param name="count">要素数。</param>
        /// <param name="comparer">比較オブジェクト。</param>
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            if (index < 0) throw new ArgumentOutOfRangeException("index");
            if (count < 0) throw new ArgumentOutOfRangeException("count");
            if (size - index < count) throw new ArgumentException("Invalid index and count.");

            Array.Sort<T>(items, index, count, comparer);
        }

        public void ForEach(Action<T> action)
        {
            if (action == null) throw new ArgumentNullException("action");

            for (int i = 0; i < size; i++)
            {
                action(items[i]);
            }
        }

        public void ForEach(RefAction<T> action)
        {
            if (action == null) throw new ArgumentNullException("action");

            for (int i = 0; i < size; i++)
            {
                action(ref items[i]);
            }
        }

        void EnsureCapacity(int min)
        {
            // List<T> と同様の容量拡張ロジック。
            if (items.Length < min)
            {
                var newCapacity = items.Length == 0 ? defaultCapacity : items.Length * 2;
                if (newCapacity < min) newCapacity = min;
                Capacity = newCapacity;
            }
        }
    }
}
