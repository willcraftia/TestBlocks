#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Collections
{
    /// <summary>
    /// 八分木で要素を管理するクラスです。
    /// この八分木では、八分木を利用する空間のスケールによらず、
    /// 八分木によるノードの分割およびノードの管理のみを実装しています。
    /// つまり、要素の取得と設定では、八分木内部で定まるノード位置 (三次元のインデックス) を用います。
    /// このため、ある空間をこの八分木実装で分割管理したい場合、
    /// その空間における位置を八分木における位置へ変換してから利用する必要があります。
    /// </summary>
    /// <typeparam name="T">リーフ ノードに設定する要素の型。</typeparam>
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

        /// <summary>
        /// 八分木の寸法。2^n でなければならない。
        /// </summary>
        int dimension;

        /// <summary>
        /// ルート ノード。
        /// </summary>
        Branch root;

        /// <summary>
        /// 指定の位置について要素を取得または設定します。
        /// </summary>
        /// <param name="point">八分木空間における位置。</param>
        /// <returns>
        /// 要素。
        /// 取得時は、要素が設定されていない場合は型 T のデフォルト。
        /// </returns>
        public T this[VectorI3 point]
        {
            get
            {
                return GetLeaf(ref point).Item;
            }
            set
            {
                GetLeaf(ref point).Item = value;
            }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="dimension">八分木の寸法。2^n でなければならない。</param>
        public Octree(int dimension)
        {
            if (dimension < 1) throw new ArgumentOutOfRangeException("dimension");
            if (((dimension - 1) & dimension) != 0) throw new ArgumentException("dimension must be a power of 2.");

            this.dimension = dimension;

            root = new Branch(VectorI3.Zero, dimension);
        }

        /// <summary>
        /// 指定の Predicate 従ってノードを探索し、リーフ ノードに到達できた場合、
        /// 指定の Action を実行します。
        /// </summary>
        /// <param name="action">到達できたリーフ ノードで実行するメソッド。</param>
        /// <param name="predicate">探索するノードを定める述語。</param>
        public void Execute(Action<Leaf> action, Predicate<Node> predicate)
        {
            if (predicate(root))
                Execute(root, action, predicate);
        }

        /// <summary>
        /// 指定の Predicate 従ってノードを探索し、リーフ ノードに到達できた場合、
        /// 指定の Action を実行します。
        /// </summary>
        /// <param name="branch">探索の基点とするブランチ ノード。</param>
        /// <param name="action">到達できたリーフ ノードで実行するメソッド。</param>
        /// <param name="predicate">探索するノードを定める述語。</param>
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

        /// <summary>
        /// 指定の位置にある要素を削除します。
        /// このメソッドは、指定の位置へ型 T のデフォルトを設定します。
        /// </summary>
        /// <param name="point">八分木空間における位置。</param>
        public void RemoveItem(VectorI3 point)
        {
            GetLeaf(ref point).Item = default(T);
        }

        /// <summary>
        /// 指定の位置にあるリーフ ノードを取得します。
        /// </summary>
        /// <param name="point">八分木空間における位置。</param>
        /// <returns>リーフ ノード。</returns>
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
