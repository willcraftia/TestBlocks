#region Using

using System;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    /// <summary>
    /// チャンク メッシュの頂およびインデックス情報を管理するクラスです。
    /// このクラスで管理する頂点は VertexPositionNormalColorTexture、インデックスは ushort です。
    /// </summary>
    public sealed class ChunkVertices
    {
        // TODO
        //
        // 実行で最適と思われる値を調べて決定する。
        // 恐らくは、最大頂点数となりうる構造は確定できるため、事前に算出できるはずだが、
        // その算出アルゴリズムが分からない。
        public const ushort VertexCapacity = 20000;

        public const ushort IndexCapacity = 6 * VertexCapacity / 4;

        /// <summary>
        /// 頂点情報。
        /// </summary>
        VertexPositionNormalColorTexture[] vertices = new VertexPositionNormalColorTexture[VertexCapacity];

        /// <summary>
        /// インデックス情報。
        /// </summary>
        ushort[] indices = new ushort[IndexCapacity];

        /// <summary>
        /// 全頂点を内包する BoundingBox。
        /// </summary>
        BoundingBox boundingBox;

        /// <summary>
        /// BoundingBox のコーナを取得するために用いるバッファ。
        /// </summary>
        Vector3[] corners = new Vector3[8];

        /// <summary>
        /// 頂点数を取得します。
        /// </summary>
        public int VertexCount { get; private set; }

        /// <summary>
        /// インデックス数を取得します。
        /// </summary>
        public int IndexCount { get; private set; }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        public ChunkVertices()
        {
            vertices = new VertexPositionNormalColorTexture[VertexCapacity];
            indices = new ushort[IndexCapacity];
        }

        /// <summary>
        /// チャンク メッシュへ頂点およびインデックス情報を設定します。
        /// </summary>
        /// <param name="mesh">チャンク メッシュ。</param>
        public void Populate(ChunkMesh mesh)
        {
            if (mesh == null) throw new ArgumentNullException("mesh");

            // メッシュの BoundingBox。
            var transform = mesh.World;
            Vector3.Transform(ref boundingBox.Min, ref transform, out mesh.BoundingBox.Min);
            Vector3.Transform(ref boundingBox.Max, ref transform, out mesh.BoundingBox.Max);

            // メッシュの BoundingSphere。
            mesh.BoundingBox.GetCorners(corners);
            mesh.BoundingSphere = BoundingSphere.CreateFromPoints(corners);

            mesh.SetVertices(vertices, VertexCount);
            mesh.SetIndices(indices, IndexCount);
        }

        /// <summary>
        /// インデックスを追加します。
        /// インデックスの追加の前には、必ず対応する頂点を追加していなければなりません。
        /// </summary>
        /// <param name="index">インデックス。</param>
        public void AddIndex(ushort index)
        {
            indices[IndexCount++] = (ushort) (VertexCount + index);
        }

        /// <summary>
        /// 頂点を追加します。
        /// </summary>
        /// <param name="vertex">頂点。</param>
        public void AddVertex(ref VertexPositionNormalColorTexture vertex)
        {
            vertices[VertexCount++] = vertex;

            // AABB を更新。
            Vector3.Max(ref boundingBox.Max, ref vertex.Position, out boundingBox.Max);
            Vector3.Min(ref boundingBox.Min, ref vertex.Position, out boundingBox.Min);
        }

        /// <summary>
        /// 頂点とインデックスの情報を初期化します。
        /// </summary>
        public void Clear()
        {
            VertexCount = 0;
            IndexCount = 0;
            boundingBox = BoundingBoxHelper.Empty;
        }
    }
}
