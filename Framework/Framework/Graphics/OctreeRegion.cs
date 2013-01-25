#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class OctreeRegion
    {
        BoundingBox box;

        int maxDepth;

        Octree root;

        public OctreeRegion(BoundingBox box, int maxDepth)
        {
            this.box = box;
            this.maxDepth = maxDepth;

            root = new Octree(null);
            root.Box = box;
        }

        public void UpdateOctreeNode(OctreeNode node)
        {
            ContainmentType containmentType;

            if (node.Octree == null)
            {
                root.Box.Contains(ref node.BoxWorld, out containmentType);
                if (containmentType != ContainmentType.Contains)
                {
                    // ルートにすら含まれない場合は、ルートへ強制登録。
                    root.Nodes.Add(node);
                }
                else
                {
                    // 適切な子を探索して登録。
                    AddOctreeNode(node, root, 0);
                }
            }

            node.Octree.Box.Contains(ref node.BoxWorld, out containmentType);
            if (containmentType != ContainmentType.Contains)
            {
                // 登録先の八分木に含まれなくなっていたならば、
                // 適切な八分木へ登録し直す。
                
                // 現在の登録先から削除。
                RemoveOctreeNode(node);

                root.Box.Contains(ref node.BoxWorld, out containmentType);
                if (containmentType != ContainmentType.Contains)
                {
                    // ルートにすら含まれない場合は、ルートへ強制登録。
                    root.Nodes.Add(node);
                }
                else
                {
                    // 適切な子を探索して登録。
                    AddOctreeNode(node, root, 0);
                }
            }
        }

        void AddOctreeNode(OctreeNode node, Octree octree, int depth)
        {
            if (depth < maxDepth && octree.IsTwiceSize(ref node.BoxWorld))
            {
                // 指定された八分木のサイズがノードの二倍以上ならば、
                // 子に対してノードを追加。

                int x;
                int y;
                int z;
                octree.GetChildIndex(ref node.BoxWorld, out x, out y, out z);

                if (octree[x, y, z] == null)
                {
                    // 子がまだ存在しないなら生成。
                    octree.AllocateChild(x, y, z);
                }

                AddOctreeNode(node, octree[x, y, z], ++depth);
            }
            else
            {
                octree.Nodes.Add(node);
            }
        }

        void RemoveOctreeNode(OctreeNode node)
        {
            if (node.Octree == null) return;

            node.Octree.Nodes.Remove(node);
        }
    }
}
