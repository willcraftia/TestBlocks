#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class Octree
    {
        #region NodeCollection

        public sealed class NodeCollection : ListBase<OctreeNode>
        {
            Octree octree;

            internal NodeCollection(Octree octree)
            {
                this.octree = octree;
            }

            protected override void InsertOverride(int index, OctreeNode item)
            {
                item.Octree = octree;

                base.InsertOverride(index, item);
            }

            protected override void SetOverride(int index, OctreeNode item)
            {
                var removed = this[index];
                removed.Octree = null;

                item.Octree = octree;

                base.SetOverride(index, item);
            }

            protected override void RemoveAtOverride(int index)
            {
                var removed = this[index];
                removed.Octree = null;

                base.RemoveAtOverride(index);
            }

            protected override void ClearOverride()
            {
                foreach (var item in this)
                    item.Octree = null;

                base.ClearOverride();
            }
        }

        #endregion

        public BoundingBox Box;

        Octree parent;

        Octree[, ,] children = new Octree[2, 2, 2];

        public Octree this[int x, int y, int z]
        {
            get { return children[x, y, z]; }
            set { children[x, y, z] = value; }
        }

        public NodeCollection Nodes { get; private set; }

        public Octree(Octree parent)
        {
            this.parent = parent;
            Nodes = new NodeCollection(this);
        }

        public bool IsTwiceSize(ref BoundingBox box)
        {
            Vector3 halfSize;
            Box.GetHalfSize(out halfSize);

            Vector3 targetSize;
            box.GetSize(out targetSize);

            return (targetSize.X <= halfSize.X) && (targetSize.Y <= halfSize.Y) && (targetSize.Z <= halfSize.Z);
        }

        public void GetChildIndex(ref BoundingBox box, out int x, out int y, out int z)
        {
            Vector3 center;
            Box.GetCenter(out center);

            Vector3 targetCenter;
            box.GetCenter(out targetCenter);

            x = (targetCenter.X <= center.X) ? 0 : 1;
            y = (targetCenter.Y <= center.Y) ? 0 : 1;
            z = (targetCenter.Z <= center.Z) ? 0 : 1;
        }

        public void AllocateChild(int x, int y, int z)
        {
            if (children[x, y, z] != null) return;

            // 子がまだ存在しないなら生成。
            var child = new Octree(this);
            children[x, y, z] = child;

            var max = Box.Max;
            var min = Box.Min;

            var childMax = new Vector3();
            var childMin = new Vector3();

            if (x == 0)
            {
                childMin.X = min.X;
                childMax.X = (min.X + max.X) / 2;
            }
            else
            {
                childMin.X = (min.X + max.X) / 2;
                childMax.X = max.X;
            }

            if (y == 0)
            {
                childMin.Y = min.Y;
                childMax.Y = (min.Y + max.Y) / 2;
            }
            else
            {
                childMin.Y = (min.Y + max.Y) / 2;
                childMax.Y = max.Y;
            }

            if (z == 0)
            {
                childMin.Z = min.Z;
                childMax.Z = (min.Z + max.Z) / 2;
            }
            else
            {
                childMin.Z = (min.Z + max.Z) / 2;
                childMax.Z = max.Z;
            }

            child.Box.Min = childMin;
            child.Box.Max = childMax;
        }
    }
}
