#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Collections
{
    /// <summary>
    /// キーが要素に埋め込まれているスレッド セーフな待ち行列です。
    /// IEnumerable&lt;T&gt; の実装は面倒なため実装しません。
    /// </summary>
    /// <typeparam name="TKey">キーの型。</typeparam>
    /// <typeparam name="TItem">要素の型。</typeparam>
    public sealed class ConcurrentKeyedQueue<TKey, TItem>
    {
        Func<TItem, TKey> getKeyFunc;

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
            get
            {
                lock (this)
                {
                    return queue.Count;
                }
            }
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
                
                lock (this)
                {
                    return dictionary[key];
                }
            }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="getKeyFunc">要素のキーを取得するデリゲート。</param>
        /// <param name="capacity">初期容量。</param>
        public ConcurrentKeyedQueue(Func<TItem, TKey> getKeyFunc, int capacity)
        {
            if (getKeyFunc == null) throw new ArgumentNullException("getKeyFunc");
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity");

            this.getKeyFunc = getKeyFunc;
            queue = new Queue<TItem>(capacity);
            dictionary = new Dictionary<TKey, TItem>(capacity);
        }

        /// <summary>
        /// 末尾に要素を追加します。
        /// </summary>
        /// <param name="item">要素。</param>
        public void Enqueue(TItem item)
        {
            if (item == null) throw new ArgumentNullException("item");

            lock (this)
            {
                var key = getKeyFunc(item);
                dictionary[key] = item;
                queue.Enqueue(item);
            }
        }

        /// <summary>
        /// 先頭の要素を削除してから取得します。
        /// </summary>
        /// <returns></returns>
        public TItem Dequeue()
        {
            lock (this)
            {
                var item = queue.Dequeue();
                var key = getKeyFunc(item);
                dictionary.Remove(key);
                return item;
            }
        }

        /// <summary>
        /// 先頭の要素を削除せずに取得します。
        /// </summary>
        /// <returns></returns>
        public TItem Peek()
        {
            lock (this)
            {
                return queue.Peek();
            }
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

            lock (this)
            {
                return dictionary.ContainsKey(key);
            }
        }

        /// <summary>
        /// 全ての要素を削除します。
        /// </summary>
        public void Clear()
        {
            lock (this)
            {
                queue.Clear();
                dictionary.Clear();
            }
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
            lock (this)
            {
                return dictionary.TryGetValue(key, out item);
            }
        }
    }
}
