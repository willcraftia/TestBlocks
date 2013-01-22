#region Using

using System;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public sealed class PartitionOctree
    {
        #region Node

        /// <summary>
        /// ノードを表すクラスです。
        /// </summary>
        public abstract class Node
        {
            /// <summary>
            /// 中心位置。
            /// </summary>
            public Vector3 Center;

            /// <summary>
            /// サイズ。
            /// </summary>
            public float Size;

            public BoundingBox Box;

            /// <summary>
            /// インスタンスを生成します。
            /// </summary>
            /// <param name="center">中心位置。</param>
            /// <param name="size">サイズ。</param>
            protected Node(Vector3 center, float size)
            {
                Center = center;
                Size = size;

                var halfSize = new Vector3(Size / 2);
                Box.Min = Center - halfSize;
                Box.Max = Center + halfSize;
            }
        }

        #endregion

        #region Branch

        /// <summary>
        /// ブランチを表すノード実装です。
        /// </summary>
        public sealed class Branch : Node
        {
            public const int ChildCount = 8;

            public readonly int Count;

            /// <summary>
            /// 子ノード。
            /// </summary>
            readonly Node[,,] children;

            /// <summary>
            /// インスタンスを生成します。
            /// </summary>
            /// <param name="center">中心位置。</param>
            /// <param name="size">サイズ。</param>
            /// <param name="leafSize">リーフ サイズ。</param>
            internal Branch(Vector3 center, float size, float leafSize)
                : base(center, size)
            {
                children = new Node[2, 2, 2];

                Count = ChildCount;

                var childSize = size / 2;

                for (int z = 0; z < 2; z++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        for (int x = 0; x < 2; x++)
                        {
                            var childCenter = Center + new Vector3(x - 0.5f, y - 0.5f, z - 0.5f) * childSize;

                            if (childSize == leafSize)
                            {
                                children[x, y, z] = new Leaf(childCenter, childSize);
                            }
                            else
                            {
                                var branch = new Branch(childCenter, childSize, leafSize);
                                children[x, y, z] = branch;
                                Count += branch.Count;
                            }
                        }
                    }
                }
            }

            public void Execute(BoundingFrustum frustum, Action<Leaf> action)
            {
                foreach (var child in children)
                {
                    ContainmentType containmentType;
                    frustum.Contains(ref child.Box, out containmentType);
                    if (containmentType == ContainmentType.Disjoint) continue;

                    var branch = child as Branch;
                    if (branch != null)
                    {
                        branch.Execute(frustum, action);
                    }
                    else
                    {
                        action(child as Leaf);
                    }
                }
            }
        }

        #endregion

        #region Leaf

        public sealed class Leaf : Node
        {
            public VectorI3 PartitionOffset;

            /// <summary>
            /// インスタンスを生成します。
            /// </summary>
            /// <param name="center">中心位置。</param>
            /// <param name="size">サイズ。</param>
            internal Leaf(Vector3 center, float size)
                : base(center, size)
            {
                // 外部で計算させる前提でフィールドを削除し、メモリを節約するか？

                PartitionOffset.X = (int) Math.Floor(center.X / size);
                PartitionOffset.Y = (int) Math.Floor(center.Y / size);
                PartitionOffset.Z = (int) Math.Floor(center.Z / size);
            }
        }

        #endregion

        /// <summary>
        /// 八分木の寸法。
        /// </summary>
        float dimension;

        /// <summary>
        /// リーフ サイズ。
        /// </summary>
        float leafSize;

        /// <summary>
        /// ルート ノード。
        /// </summary>
        Branch root;

        BoundingFrustum frustum;

        // 寸法は、512、256、128・・・現実的には 512 か 256。
        // 1024、2048 は八分木が巨大になり過ぎる。
        // 固定サイズとして 512 として良いかもしれない。

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="dimension">八分木の寸法。</param>
        /// <param name="leafSize">リーフ サイズ。</param>
        public PartitionOctree(float dimension, float leafSize)
        {
            if (dimension <= 0) throw new ArgumentOutOfRangeException("dimension");
            if (leafSize <= 0) throw new ArgumentOutOfRangeException("leafSize");

            this.dimension = dimension;
            this.leafSize = leafSize;

            // ルートの中心はゼロ。
            root = new Branch(Vector3.Zero, dimension, leafSize);

            frustum = new BoundingFrustum(Matrix.Identity);
        }

        public void Update(Matrix view, Matrix projection)
        {
            // 視点を八分木の中心位置へ移動させ、境界錐台を構築。
            view.Translation = Vector3.Zero;
            frustum.Matrix = view * projection;
        }

        public bool Contains(Vector3 point)
        {
            return root.Box.Contains(point) != ContainmentType.Disjoint;
        }

        public void Execute(Action<Leaf> action)
        {
            // 中心位置から見た境界錐台と交差するノードを探索。
            root.Execute(frustum, action);
        }
    }
}
