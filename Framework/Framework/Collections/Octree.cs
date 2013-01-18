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
        abstract class Node
        {
            /// <summary>
            /// 八分木空間における原点位置。
            /// </summary>
            protected readonly VectorI3 Origin;

            /// <summary>
            /// 八分木空間におけるサイズ。
            /// </summary>
            protected readonly int Size;

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
            /// 原点に最も近いノードが [0,0,0]、
            /// 原点から最も遠いノードが [1,1,1]。
            /// </summary>
            readonly Node[,,] children;

            /// <summary>
            /// インスタンスを生成します。
            /// </summary>
            /// <param name="origin">八分木空間における原点位置。</param>
            /// <param name="size">八分木空間におけるサイズ。</param>
            internal Branch(VectorI3 origin, int size)
                : base(origin, size)
            {
                children = new Node[2, 2, 2];

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
                                children[x, y, z] = new Leaf(childOrigin, childSize);
                            }
                            else
                            {
                                children[x, y, z] = new Branch(childOrigin, childSize);
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
            internal Node GetChild(ref VectorI3 point)
            {
                var relative = point - Origin;
                var halfSize = Size / 2;
                var x = relative.X / halfSize;
                var y = relative.Y / halfSize;
                var z = relative.Z / halfSize;

                return children[x, y, z];
            }
        }

        #endregion

        #region Leaf

        sealed class Leaf : Node
        {
            /// <summary>
            /// 要素。
            /// </summary>
            internal T Item;

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

        // 原点もほぼ整数で扱うと思うが、
        // 要素取得で使用する座標が float であるため、計算のしやすさから float。
        
        /// <summary>
        /// ワールド空間における八分木の原点位置。
        /// </summary>
        Vector3 origin;

        // 整数で寸法を決めたいので int。float にすると計算が面倒。

        /// <summary>
        /// ワールド空間における八分木の寸法。
        /// </summary>
        int dimension;

        // 整数で要素サイズを決めたいので int。float にすると計算が面倒。

        /// <summary>
        /// ワールド空間における要素のサイズ (リーフ サイズ)。
        /// </summary>
        int itemSize;

        /// <summary>
        /// ルート ノード。
        /// </summary>
        Node root;

        /// <summary>
        /// ワールド空間における八分木の原点位置を取得または設定します。
        /// </summary>
        public Vector3 Origin
        {
            get { return origin; }
            set { origin = value; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="dimension">ワールド空間における八分木の寸法。</param>
        /// <param name="itemSize">ワールド空間における要素のサイズ (リーフ サイズ)。</param>
        public Octree(int dimension, int itemSize)
        {
            if (dimension < 1) throw new ArgumentOutOfRangeException("dimension");
            if (itemSize < 1) throw new ArgumentOutOfRangeException("itemSize");

            this.dimension = dimension;
            this.itemSize = itemSize;

            root = new Branch(VectorI3.Zero, dimension / itemSize);
        }

        /// <summary>
        /// ワールド空間における指定の位置が八分木に含まれるか否かを判定します。
        /// </summary>
        /// <param name="position">ワールド空間における位置。</param>
        /// <returns>
        /// true (八分木に含まれる場合)、false (それ以外の場合)。
        /// </returns>
        public bool Contains(Vector3 position)
        {
            bool result;
            Contains(ref position, out result);
            return result;
        }

        /// <summary>
        /// ワールド空間における指定の位置が八分木に含まれるか否かを判定します。
        /// </summary>
        /// <param name="position">ワールド空間における位置。</param>
        /// <param name="result">
        /// true (八分木に含まれる場合)、false (それ以外の場合)。
        /// </param>
        public void Contains(ref Vector3 position, out bool result)
        {
            // ワールド空間における八分木の原点からの相対位置。
            var relative = position - origin;

            // 範囲外か否か。
            if (relative.X < 0 || dimension <= relative.X ||
                relative.Y < 0 || dimension <= relative.Y ||
                relative.Z < 0 || dimension <= relative.Z)
            {
                result = false;
            }
            else
            {
                result = true;
            }
        }

        /// <summary>
        /// ワールド空間における指定の位置にある要素を取得します。
        /// </summary>
        /// <param name="position">ワールド空間における位置。</param>
        /// <returns>要素。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// position が八分木の範囲外である場合。
        /// </exception>
        public T GetItem(Vector3 position)
        {
            T result;
            GetItem(ref position, out result);
            return result;
        }

        /// <summary>
        /// ワールド空間における指定の位置にある要素を取得します。
        /// </summary>
        /// <param name="position">ワールド空間における位置。</param>
        /// <param name="result">要素。</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// position が八分木の範囲外である場合。
        /// </exception>
        public void GetItem(ref Vector3 position, out T result)
        {
            // ワールド空間における八分木の原点からの相対位置。
            var relative = position - origin;

            // 範囲外か否か。
            if (relative.X < 0 || dimension <= relative.X ||
                relative.Y < 0 || dimension <= relative.Y ||
                relative.Z < 0 || dimension <= relative.Z)
                throw new ArgumentOutOfRangeException("position");

            // 八分木空間での位置。
            var point = new VectorI3
            {
                X = (int) (relative.X / itemSize),
                Y = (int) (relative.Y / itemSize),
                Z = (int) (relative.Z / itemSize)
            };

            var node = root;

            while (true)
            {
                var branch = node as Branch;
                if (branch != null)
                {
                    node = branch.GetChild(ref point);
                }
                else
                {
                    result = (node as Leaf).Item;
                    return;
                }
            }
        }

        /// <summary>
        /// ワールド空間における指定の位置に要素を設定します。
        /// </summary>
        /// <param name="position">ワールド空間における位置。</param>
        /// <param name="item">要素。</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// position が八分木の範囲外である場合。
        /// </exception>
        public void SetItem(Vector3 position, T item)
        {
            SetItem(ref position, ref item);
        }

        /// <summary>
        /// ワールド空間における指定の位置に要素を設定します。
        /// </summary>
        /// <param name="position">ワールド空間における位置。</param>
        /// <param name="item">要素。</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// position が八分木の範囲外である場合。
        /// </exception>
        public void SetItem(ref Vector3 position, ref T item)
        {
            // ワールド空間における八分木の原点からの相対位置。
            var relative = position - origin;

            // 範囲外か否か。
            if (relative.X < 0 || dimension <= relative.X ||
                relative.Y < 0 || dimension <= relative.Y ||
                relative.Z < 0 || dimension <= relative.Z)
                throw new ArgumentOutOfRangeException("position");

            // 八分木空間での位置。
            var point = new VectorI3
            {
                X = (int) (relative.X / itemSize),
                Y = (int) (relative.Y / itemSize),
                Z = (int) (relative.Z / itemSize)
            };

            var node = root;

            while (true)
            {
                var branch = node as Branch;
                if (branch != null)
                {
                    node = branch.GetChild(ref point);
                }
                else
                {
                    (node as Leaf).Item = item;
                    return;
                }
            }
        }

        /// <summary>
        /// ワールド空間における指定の位置にある要素を削除します。
        /// </summary>
        /// <param name="position">ワールド空間における位置。</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// position が八分木の範囲外である場合。
        /// </exception>
        public void Remove(Vector3 position)
        {
            Remove(ref position);
        }

        /// <summary>
        /// ワールド空間における指定の位置にある要素を削除します。
        /// </summary>
        /// <param name="position">ワールド空間における位置。</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// position が八分木の範囲外である場合。
        /// </exception>
        public void Remove(ref Vector3 position)
        {
            // ワールド空間における八分木の原点からの相対位置。
            var relative = position - origin;

            // 範囲外か否か。
            if (relative.X < 0 || dimension <= relative.X ||
                relative.Y < 0 || dimension <= relative.Y ||
                relative.Z < 0 || dimension <= relative.Z)
                throw new ArgumentOutOfRangeException("position");

            // 八分木空間での位置。
            var point = new VectorI3
            {
                X = (int) (relative.X / itemSize),
                Y = (int) (relative.Y / itemSize),
                Z = (int) (relative.Z / itemSize)
            };

            var node = root;

            while (true)
            {
                var branch = node as Branch;
                if (branch != null)
                {
                    node = branch.GetChild(ref point);
                }
                else
                {
                    (node as Leaf).Item = default(T);
                    return;
                }
            }
        }
    }
}
