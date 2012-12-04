#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Collections
{
    /// <summary>
    /// キーが値に埋め込まれているリストの抽象基本クラスです。
    /// System.Collections.ObjectModel.KeyedCollection とは異なり、
    /// 値のキーが null であることを許容しません。
    /// また、初期容量をコンストラクタで指定でき、
    /// インスタンス後も Capacity プロパティによりリストの容量を任意に変更できます。
    /// ただし、キー管理のための Dictionary の容量は、Dictionary に従います。
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public abstract class KeyedList<TKey, TItem> : ListBase<TItem>
    {
        Dictionary<TKey, TItem> dictionary;

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

        protected KeyedList()
        {
            dictionary = new Dictionary<TKey, TItem>(Capacity);
        }

        protected KeyedList(int capacity)
            : base(capacity)
        {
            dictionary = new Dictionary<TKey, TItem>(Capacity);
        }

        public bool Contains(TKey key)
        {
            if (key == null) throw new ArgumentNullException("key");

            return dictionary.ContainsKey(key);
        }

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

        public bool TryGet(TKey key, out TItem item)
        {
            return dictionary.TryGetValue(key, out item);
        }

        protected abstract TKey GetKeyForItem(TItem item);

        protected override void ClearOverride()
        {
            dictionary.Clear();

            base.ClearOverride();
        }

        protected override void InsertOverride(int index, TItem item)
        {
            var key = GetKey(item);
            dictionary[key] = item;

            base.InsertOverride(index, item);
        }

        protected override void SetOverride(int index, TItem item)
        {
            var newKey = GetKey(item);
            var oldKey = GetKey(base[index]);

            if (!oldKey.Equals(newKey)) dictionary.Remove(oldKey);

            dictionary[newKey] = item;

            base.SetOverride(index, item);
        }

        protected override void RemoveAtOverride(int index)
        {
            var key = GetKey(base[index]);
            dictionary.Remove(key);

            base.RemoveAtOverride(index);
        }

        protected TKey GetKey(TItem item)
        {
            var key = GetKeyForItem(item);
            if (key == null) throw new InvalidOperationException("The item has no key.");

            return key;
        }
    }
}
