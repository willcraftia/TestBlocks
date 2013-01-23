
#region Using

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Collections
{
    /// <summary>
    /// リスト構造クラスの実装のための抽象基本クラスです。
    /// </summary>
    /// <typeparam name="T">要素の型。</typeparam>
    public class ListBase<T> : IList<T>
    {
        // List のデフォルト値と同じ。
        const int defaultCapacity = 4;

        /// <summary>
        /// リスト。
        /// </summary>
        List<T> list;

        /// <summary>
        /// 容量を取得または設定します。
        /// </summary>
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

        /// <summary>
        /// インスタンスを生成します。
        /// 初期容量には 4 を用います。
        /// </summary>
        public ListBase()
            : this(defaultCapacity)
        {
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="capacity">初期容量。</param>
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

        /// <summary>
        /// 要素をソートします。
        /// </summary>
        public void Sort()
        {
            list.Sort();
        }

        /// <summary>
        /// 指定の比較デリゲートを用いて要素をソートします。
        /// </summary>
        /// <param name="comparison"></param>
        public void Sort(Comparison<T> comparison)
        {
            list.Sort(comparison);
        }

        /// <summary>
        /// 指定の比較オブジェクトを用いて要素をソートします。
        /// </summary>
        /// <param name="comparer">比較オブジェクト。</param>
        public void Sort(IComparer<T> comparer)
        {
            list.Sort(comparer);
        }

        /// <summary>
        /// 指定の範囲にある要素をソートします。
        /// </summary>
        /// <param name="index">開始位置。</param>
        /// <param name="count">要素数。</param>
        /// <param name="comparer">比較オブジェクト。</param>
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            list.Sort(index, count, comparer);
        }

        /// <summary>
        /// 全ての要素を削除する際に呼び出されます。
        /// </summary>
        protected virtual void ClearOverride()
        {
            list.Clear();
        }

        /// <summary>
        /// 指定の位置へ要素を挿入する際に呼び出されます。
        /// </summary>
        /// <param name="index">インデックス。</param>
        /// <param name="item">要素。</param>
        protected virtual void InsertOverride(int index, T item)
        {
            list.Insert(index, item);
        }

        /// <summary>
        /// 指定の位置へ要素を設定する際に呼び出されます。
        /// </summary>
        /// <param name="index">インデックス。</param>
        /// <param name="item">要素。</param>
        protected virtual void SetOverride(int index, T item)
        {
            list[index] = item;
        }

        /// <summary>
        /// 指定の位置から要素を削除する際に呼び出されます。
        /// </summary>
        /// <param name="index">インデックス。</param>
        protected virtual void RemoveAtOverride(int index)
        {
            list.RemoveAt(index);
        }
    }
}
