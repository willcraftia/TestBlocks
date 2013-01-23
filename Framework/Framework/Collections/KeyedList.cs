#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Collections
{
    /// <summary>
    /// キーが要素に埋め込まれているリストの抽象基本クラスです。
    /// System.Collections.ObjectModel.KeyedCollection とは異なり、
    /// キーが null であることを許容しません。
    /// また、初期容量をコンストラクタで指定でき、
    /// インスタンス後も Capacity プロパティによりリストの容量を任意に変更できます。
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public abstract class KeyedList<TKey, TItem> : ListBase<TItem>
    {
        /// <summary>
        /// キーと要素のディクショナリ。
        /// </summary>
        Dictionary<TKey, TItem> dictionary;

        /// <summary>
        /// 指定のキーに対応する要素を取得します。
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
        /// 初期容量には 4 を用います。
        /// </summary>
        protected KeyedList()
        {
            dictionary = new Dictionary<TKey, TItem>(Capacity);
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="capacity">初期容量。</param>
        protected KeyedList(int capacity)
            : base(capacity)
        {
            dictionary = new Dictionary<TKey, TItem>(Capacity);
        }

        /// <summary>
        /// 指定のキーに対応する要素が存在するか否かを検査します。
        /// </summary>
        /// <param name="key">キー。</param>
        /// <returns>
        /// true (要素が存在する場合)、false (それ以外の場合)。
        /// </returns>
        public bool Contains(TKey key)
        {
            if (key == null) throw new ArgumentNullException("key");

            return dictionary.ContainsKey(key);
        }

        /// <summary>
        /// 指定のキーに対応する要素を削除します。
        /// </summary>
        /// <param name="key">キー。</param>
        /// <returns>
        /// true (要素が存在しない場合)、false (それ以外の場合)。
        /// </returns>
        public bool Remove(TKey key)
        {
            if (key == null) throw new ArgumentNullException("key");

            TItem item;
            if (dictionary.TryGetValue(key, out item))
            {
                dictionary.Remove(key);
                return Remove(item);
            }

            return false;
        }

        /// <summary>
        /// 指定のキーに対応する要素の取得を試行します。
        /// </summary>
        /// <param name="key">キー。</param>
        /// <param name="item">要素。</param>
        /// <returns>
        /// true (要素が存在しない場合)、false (それ以外の場合)。
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
        /// ディクショナリから全ての要素を削除します。
        /// </summary>
        protected override void ClearOverride()
        {
            dictionary.Clear();

            base.ClearOverride();
        }

        /// <summary>
        /// ディクショナリへ要素を追加します。
        /// </summary>
        protected override void InsertOverride(int index, TItem item)
        {
            var key = GetKey(item);
            dictionary[key] = item;

            base.InsertOverride(index, item);
        }

        /// <summary>
        /// 要素のキーに従ってディクショナリの要素を置換します。
        /// </summary>
        protected override void SetOverride(int index, TItem item)
        {
            var newKey = GetKey(item);
            var oldKey = GetKey(base[index]);

            if (!oldKey.Equals(newKey)) dictionary.Remove(oldKey);

            dictionary[newKey] = item;

            base.SetOverride(index, item);
        }

        /// <summary>
        /// ディクショナリから要素を削除します。
        /// </summary>
        protected override void RemoveAtOverride(int index)
        {
            var key = GetKey(base[index]);
            dictionary.Remove(key);

            base.RemoveAtOverride(index);
        }

        /// <summary>
        /// 指定した要素からキーを抽出します。
        /// </summary>
        /// <param name="item">要素。</param>
        /// <returns>キー。</returns>
        protected TKey GetKey(TItem item)
        {
            var key = GetKeyForItem(item);
            if (key == null) throw new InvalidOperationException("The item has no key.");

            return key;
        }
    }
}
