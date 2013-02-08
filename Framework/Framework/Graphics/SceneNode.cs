#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public class SceneNode
    {
        #region ChildCollection

        public sealed class ChildCollection : ICollection<SceneNode>
        {
            SceneNode parent;

            Dictionary<string, SceneNode> dictionary = new Dictionary<string, SceneNode>();

            public SceneNode this[string name]
            {
                get { return dictionary[name]; }
            }

            internal ChildCollection(SceneNode parent)
            {
                this.parent = parent;
            }

            #region ICollection

            public void Add(SceneNode item)
            {
                if (item == null) throw new ArgumentNullException("item");
                if (item.Parent != null) throw new ArgumentException(string.Format(
                    "Node '{0}' already was a child of '{1}'.", item.Name, item.Parent.Name));

                dictionary[item.Name] = item;
                item.Parent = parent;
            }

            public void Clear()
            {
                foreach (var item in dictionary.Values)
                {
                    // 八分木から削除。
                    RemoveOctreeSceneNode(item);

                    item.Parent = null;
                }

                dictionary.Clear();
            }

            public bool Contains(SceneNode item)
            {
                if (item == null) throw new ArgumentNullException("item");

                return dictionary.ContainsKey(item.Name);
            }

            public void CopyTo(SceneNode[] array, int arrayIndex)
            {
                if (array == null) throw new ArgumentNullException("array");
                if (arrayIndex < 0 || array.Length <= arrayIndex) throw new ArgumentOutOfRangeException("arrayIndex");

                dictionary.Values.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(SceneNode item)
            {
                if (item == null) throw new ArgumentNullException("item");

                // 八分木から削除。
                RemoveOctreeSceneNode(item);

                item.Parent = null;
                return dictionary.Remove(item.Name);
            }

            public IEnumerator<SceneNode> GetEnumerator()
            {
                return dictionary.Values.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            public bool Contains(string name)
            {
                return dictionary.ContainsKey(name);
            }

            public bool Remove(string name)
            {
                SceneNode node;
                if (!dictionary.TryGetValue(name, out node)) return false;

                return Remove(node);
            }

            void RemoveOctreeSceneNode(SceneNode item)
            {
                // 自分の削除。
                item.Manager.RemoveOctreeSceneNode(item);

                // 子も全て八分木から削除。
                foreach (var child in item.Children)
                    RemoveOctreeSceneNode(child);
            }
        }

        #endregion

        #region ObjectCollection

        public sealed class ObjectCollection : ICollection<SceneObject>
        {
            SceneNode parent;

            Dictionary<string, SceneObject> dictionary = new Dictionary<string, SceneObject>();

            public SceneObject this[string name]
            {
                get { return dictionary[name]; }
            }

            internal ObjectCollection(SceneNode parent)
            {
                this.parent = parent;
            }

            public bool Contains(string name)
            {
                return dictionary.ContainsKey(name);
            }

            public bool Remove(string name)
            {
                SceneObject obj;
                if (!dictionary.TryGetValue(name, out obj)) return false;

                return Remove(obj);
            }

            #region ICollection

            public void Add(SceneObject item)
            {
                if (item == null) throw new ArgumentNullException("item");
                if (item.Parent != null) throw new ArgumentException(string.Format(
                    "SceneObject '{0}' already was a child of '{1}'.", item.Name, item.Parent.Name));
                if (dictionary.ContainsKey(item.Name)) throw new ArgumentException("Name duplicate: " + item.Name);

                item.Parent = parent;

                dictionary[item.Name] = item;
            }

            public void Clear()
            {
                foreach (var item in dictionary.Values)
                    item.Parent = null;

                dictionary.Clear();
            }

            public bool Contains(SceneObject item)
            {
                if (item == null) throw new ArgumentNullException("item");

                return dictionary.ContainsKey(item.Name);
            }

            public void CopyTo(SceneObject[] array, int arrayIndex)
            {
                if (array == null) throw new ArgumentNullException("array");
                if (arrayIndex < 0 || array.Length <= arrayIndex) throw new ArgumentOutOfRangeException("arrayIndex");

                dictionary.Values.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(SceneObject item)
            {
                if (item == null) throw new ArgumentNullException("item");

                item.Parent = null;

                return dictionary.Remove(item.Name);
            }

            public IEnumerator<SceneObject> GetEnumerator()
            {
                return dictionary.Values.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        #endregion

        public BoundingBox BoxWorld;

        public ChildCollection Children { get; private set; }

        public ObjectCollection Objects { get; private set; }

        public string Name { get; private set; }

        public SceneNode Parent { get; private set; }

        public SceneManager Manager { get; private set; }

        public Octree Octree { get; internal set; }

        public SceneNode(SceneManager manager, string name)
        {
            if (manager == null) throw new ArgumentNullException("manager");
            if (name == null) throw new ArgumentNullException("name");

            Manager = manager;
            Name = name;

            Children = new ChildCollection(this);
            Objects = new ObjectCollection(this);
        }

        public void SetVisible(bool visible)
        {
            SetVisible(visible, true);
        }

        public void SetVisible(bool visible, bool cascade)
        {
            foreach (var obj in Objects)
            {
                obj.Visible = visible;
            }

            foreach (var child in Children)
            {
                child.SetVisible(visible, cascade);
            }
        }

        public void Update(bool updateChildren)
        {
            if (updateChildren)
            {
                foreach (var child in Children)
                    child.Update(true);
            }

            UpdateBoxWorld();
        }

        void UpdateBoxWorld()
        {
            BoxWorld = BoundingBoxHelper.Empty;

            foreach (var obj in Objects)
            {
                BoxWorld.Min.X = MathHelper.Min(BoxWorld.Min.X, obj.BoxWorld.Min.X);
                BoxWorld.Min.Y = MathHelper.Min(BoxWorld.Min.Y, obj.BoxWorld.Min.Y);
                BoxWorld.Min.Z = MathHelper.Min(BoxWorld.Min.Z, obj.BoxWorld.Min.Z);

                BoxWorld.Max.X = MathHelper.Max(BoxWorld.Max.X, obj.BoxWorld.Max.X);
                BoxWorld.Max.Y = MathHelper.Max(BoxWorld.Max.Y, obj.BoxWorld.Max.Y);
                BoxWorld.Max.Z = MathHelper.Max(BoxWorld.Max.Z, obj.BoxWorld.Max.Z);
            }

            Manager.UpdateOctreeSceneNode(this);
        }
    }
}
