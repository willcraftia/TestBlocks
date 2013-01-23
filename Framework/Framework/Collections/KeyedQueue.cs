#region Using

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Collections
{
    /// <summary>
    /// キーで要素を管理する待ち行列の抽象基本クラスです。
    /// </summary>
    /// <typeparam name="TKey">キーの型。</typeparam>
    /// <typeparam name="TItem">要素の型。</typeparam>
    public abstract class KeyedQueue<TKey, TItem> : IEnumerable<TItem>
    {
        /// <summary>
        /// 待ち行列。
        /// </summary>
        Queue<TItem> queue;

        /// <summary>
        /// キーと要素のディクショナリ。
        /// </summary>
        Dictionary<TKey, TItem> dictionary;

        /// <summary>
        /// 要素の数を取得します。
        /// </summary>
        public int Count
        {
            get { return queue.Count; }
        }

        /// <summary>
        /// 指定のキーに対応する要素を取得します。
        /// 存在しないキーを指定した場合には例外が発生します。
        /// </summary>
        /// <param name="key">キー。</param>
        /// <returns>要素。</returns>
        public TItem this[TKey key]
        {
            get
            {
                if (key == null) throw new ArgumentNullException("key");
                return dictionary[key];
            }
        }

        /// <summary>
        /// キーと要素のディクショナリを取得します。
        /// </summary>
        protected IDictionary<TKey, TItem> Dictionary
        {
            get { return dictionary; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="capacity">初期容量。</param>
        protected KeyedQueue(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity");

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

        /// <summary>
        /// 末尾に要素を追加します。
        /// </summary>
        /// <param name="item">要素。</param>
        public void Enqueue(TItem item)
        {
            if (item == null) throw new ArgumentNullException("item");

            var key = GetKey(item);
            dictionary[key] = item;
            queue.Enqueue(item);
        }

        /// <summary>
        /// 先頭の要素を削除してから取得します。
        /// </summary>
        /// <returns></returns>
        public TItem Dequeue()
        {
            var item = queue.Dequeue();
            var key = GetKey(item);
            dictionary.Remove(key);
            return item;
        }

        /// <summary>
        /// 先頭の要素を削除せずに取得します。
        /// </summary>
        /// <returns></returns>
        public TItem Peek()
        {
            return queue.Peek();
        }

        /// <summary>
        /// 指定のキーに対応する要素が存在するか否かを検査します。
        /// </summary>
        /// <param name="key">キー。</param>
        /// <returns>
        /// true (指定のキーに対応する要素が存在する場合)、false (それ以外の場合)。
        /// </returns>
        public bool Contains(TKey key)
        {
            if (key == null) throw new ArgumentNullException("key");

            return dictionary.ContainsKey(key);
        }

        /// <summary>
        /// 全ての要素を削除します。
        /// </summary>
        public void Clear()
        {
            queue.Clear();
            dictionary.Clear();
        }

        /// <summary>
        /// 指定のキーに対応する要素の取得を試行します。
        /// </summary>
        /// <param name="key">キー。</param>
        /// <param name="item">
        /// 要素、あるいは、指定のキーに対応する要素が存在しない場合は null。
        /// </param>
        /// <returns>
        /// true (指定のキーに対応する要素が存在する場合)、false (それ以外の場合)。
        /// </returns>
        public bool TryGet(TKey key, out TItem item)
        {
            return dictionary.TryGetValue(key, out item);
        }

        /// <summary>
        /// 指定した要素からキーを抽出します。
        /// サブクラスでは、このメソッドを要素の型に応じて実装します。
        /// </summary>
        /// <param name="item">要素。</param>
        /// <returns>キー。</returns>
        protected abstract TKey GetKeyForItem(TItem item);

        /// <summary>
        /// 指定した要素からキーを抽出します。
        /// </summary>
        /// <param name="item">要素。</param>
        /// <returns>キー。</returns>
        protected TKey GetKey(TItem item)
        {
            var key = GetKeyForItem(item);
            if (key == null) throw new InvalidOperationException("Key is null.");

            return key;
        }
    }
}
