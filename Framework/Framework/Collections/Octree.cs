#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Collections
{
    public sealed class Octree<T>
    {
        #region Node

        /// <summary>
        /// ノードを表すクラスです。
        /// </summary>
        public abstract class Node
        {
            /// <summary>
            /// 八分木空間における原点位置。
            /// </summary>
            public readonly VectorI3 Origin;

            /// <summary>
            /// 八分木空間におけるサイズ。
            /// </summary>
            public readonly int Size;

            /// <summary>
            /// インスタンスを生成します。
            /// </summary>
            /// <param name="origin">八分木空間における原点位置。</param>
            /// <param name="size">八分木空間におけるサイズ。</param>
            protected Node(VectorI3 origin, int size)
            {
                Origin = origin;
                Size = size;
            }
        }

        #endregion

        #region Branch

        /// <summary>
        /// ブランチを表すノード実装です。
        /// </summary>
        sealed class Branch : Node
        {
            /// <summary>
            /// 子ノード。
            /// 原点に最も近いノードが (0,0,0)、
            /// 原点から最も遠いノードが (1,1,1)。
            /// </summary>
            internal readonly Node[, ,] Children;

            /// <summary>
            /// インスタンスを生成します。
            /// </summary>
            /// <param name="origin">八分木空間における原点位置。</param>
            /// <param name="size">八分木空間におけるサイズ。</param>
            internal Branch(VectorI3 origin, int size)
                : base(origin, size)
            {
                Children = new Node[2, 2, 2];

                var childSize = size / 2;

                for (int z = 0; z < 2; z++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        for (int x = 0; x < 2; x++)
                        {
                            var childOrigin = Origin + new VectorI3(x, y, z) * childSize;

                            if (childSize == 1)
                            {
                                Children[x, y, z] = new Leaf(childOrigin, childSize);
                            }
                            else
                            {
                                Children[x, y, z] = new Branch(childOrigin, childSize);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// 八分木空間における指定の位置にある子ノードを取得します。
            /// </summary>
            /// <param name="point">八分木空間における子ノードの位置。</param>
            /// <returns>子ノード。</returns>
            internal Node GetChild(VectorI3 point)
            {
                var relative = point - Origin;
                var halfSize = Size / 2;
                var x = relative.X / halfSize;
                var y = relative.Y / halfSize;
                var z = relative.Z / halfSize;

                return Children[x, y, z];
            }
        }

        #endregion

        #region Leaf

        public sealed class Leaf : Node
        {
            /// <summary>
            /// 要素。
            /// </summary>
            public T Item;

            /// <summary>
            /// インスタンスを生成します。
            /// </summary>
            /// <param name="origin">八分木空間における原点位置。</param>
            /// <param name="size">八分木空間におけるサイズ。</param>
            internal Leaf(VectorI3 origin, int size)
                : base(origin, size)
            {
            }
        }

        #endregion

        // 整数で寸法を決めたいので int。float にすると計算が面倒。

        /// <summary>
        /// ワールド空間における八分木の寸法。
        /// 2^n でなければならない。
        /// </summary>
        int dimension;

        // 整数で要素サイズを決めたいので int。float にすると計算が面倒。

        /// <summary>
        /// ルート ノード。
        /// </summary>
        Branch root;

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="dimension">ワールド空間における八分木の寸法。</param>
        public Octree(int dimension)
        {
            if (dimension < 1) throw new ArgumentOutOfRangeException("dimension");
            if (((dimension - 1) & dimension) != 0) throw new ArgumentException("dimension must be a power of 2.");

            this.dimension = dimension;

            root = new Branch(VectorI3.Zero, dimension);
        }

        public void Execute(Action<Leaf> action, Predicate<Node> predicate)
        {
            if (predicate(root))
                Execute(root, action, predicate);
        }

        void Execute(Branch branch, Action<Leaf> action, Predicate<Node> predicate)
        {
            foreach (var child in branch.Children)
            {
                if (predicate(child))
                {
                    var leaf = child as Leaf;
                    if (leaf != null)
                    {
                        action(leaf);
                    }
                    else
                    {
                        Execute(child as Branch, action, predicate);
                    }
                }
            }
        }

        public T GetItem(VectorI3 point)
        {
            return GetLeaf(ref point).Item;
        }

        public void SetItem(VectorI3 point, T item)
        {
            GetLeaf(ref point).Item = item;
        }

        public void RemoveItem(VectorI3 point)
        {
            GetLeaf(ref point).Item = default(T);
        }

        Leaf GetLeaf(ref VectorI3 point)
        {
            if (point.X < 0 || dimension <= point.X) throw new ArgumentOutOfRangeException("point");
            if (point.Y < 0 || dimension <= point.Y) throw new ArgumentOutOfRangeException("point");
            if (point.Z < 0 || dimension <= point.Z) throw new ArgumentOutOfRangeException("point");

            Node node = root;
            while (true)
            {
                var branch = node as Branch;
                if (branch != null)
                {
                    node = branch.GetChild(point);
                }
                else
                {
                    return (node as Leaf);
                }
            }
        }
    }
}
