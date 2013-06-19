#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    /// <summary>
    /// チャンク メッシュの頂点構築およびバッファへの反映を管理するクラスです。
    /// </summary>
    public sealed class ChunkMeshManager
    {
        /// <summary>
        /// 頂点構築要求を表すクラスです。
        /// </summary>
        class BuildVertexRequest
        {
            internal Chunk Chunk;

            internal ChunkMeshUpdatePriority Priority;

            internal TimeSpan RequestTime;

            internal ChunkVertexBuilder VertexBuilder;

            internal void Initialize(Chunk chunk, ChunkMeshUpdatePriority priority)
            {
                Chunk = chunk;
                Priority = priority;
                RequestTime = TimeSpan.FromTicks(Environment.TickCount);
            }

            internal void AttachVertexBuilder(ChunkVertexBuilder vertexBuilder)
            {
                VertexBuilder = vertexBuilder;
            }

            internal void Clear()
            {
                Chunk = null;
                VertexBuilder = null;
            }
        }

        /// <summary>
        /// バッファ反映要求を表す構造体です。
        /// </summary>
        struct UpdateBufferRequest
        {
            internal ChunkVertexBuilder VertexBuilder;

            internal IntVector3 Segment;

            internal bool Translucece;

            internal ChunkMeshUpdatePriority Priority;

            internal TimeSpan RequestTime;
        }

        /// <summary>
        /// 頂点構築要求を優先度で比較するクラスです。
        /// </summary>
        class BuildVertexRequestComparer : IComparer<BuildVertexRequest>
        {
            public int Compare(BuildVertexRequest x, BuildVertexRequest y)
            {
                var byPriority = x.Priority.CompareTo(y.Priority);
                if (byPriority != 0)
                    return byPriority;

                return x.RequestTime.CompareTo(y.RequestTime);
            }
        }

        /// <summary>
        /// バッファ反映要求を優先度で比較するクラスです。
        /// </summary>
        class UpdateBufferRequestComparer : IComparer<UpdateBufferRequest>
        {
            public int Compare(UpdateBufferRequest x, UpdateBufferRequest y)
            {
                var byPriority = x.Priority.CompareTo(y.Priority);
                if (byPriority != 0)
                    return byPriority;

                return x.RequestTime.CompareTo(y.RequestTime);
            }
        }

        /// <summary>
        /// 頂点構築を処理するクラスです。
        /// </summary>
        class BuildVertexTask
        {
            internal delegate void Callback(BuildVertexRequest request);

            ChunkMeshManager manager;

            Callback callback;

            internal WaitCallback WaitCallbackMethod;

            internal BuildVertexTask(ChunkMeshManager manager, Callback callback)
            {
                this.manager = manager;
                this.callback = callback;

                WaitCallbackMethod = new WaitCallback(Execute);
            }

            void Execute(object state)
            {
                var request = state as BuildVertexRequest;

                var vertexBuilder = request.VertexBuilder;

                vertexBuilder.Initialize(request.Chunk);
                vertexBuilder.Execute();

                callback(request);
            }
        }

        /// <summary>
        /// メッシュ サイズ。
        /// </summary>
        public static readonly IntVector3 MeshSize = new IntVector3(16);

        /// <summary>
        /// チャンク サイズ。
        /// </summary>
        /// <remarks>
        /// チャンク サイズは、メッシュ サイズの等倍でなければなりません。
        /// </remarks>
        public readonly IntVector3 ChunkSize;

        /// <summary>
        /// セグメント サイズ。
        /// </summary>
        /// <remarks>
        /// 指定されたセグメント サイズに従い、
        /// チャンクのメッシュは複数のメッシュへ分割されます。
        /// </remarks>
        public readonly IntVector3 MeshSegments;

        /// <summary>
        /// チャンク メッシュのオフセット。
        /// </summary>
        public readonly Vector3 MeshOffset;

        /// <summary>
        /// チャンク マネージャ。
        /// </summary>
        ChunkManager chunkManager;

        /// <summary>
        /// 頂点構築要求のプール。
        /// </summary>
        ConcurrentPool<BuildVertexRequest> buildVertexRequestPool;

        /// <summary>
        /// 頂点ビルダのプール。
        /// </summary>
        ConcurrentPool<ChunkVertexBuilder> vertexBuilderPool;

        /// <summary>
        /// 頂点構築要求のキュー。
        /// </summary>
        ConcurrentPriorityQueue<BuildVertexRequest> buildVertexRequests;

        /// <summary>
        /// バッファ反映要求のキュー。
        /// </summary>
        ConcurrentPriorityQueue<UpdateBufferRequest> updateBufferRequests;

        /// <summary>
        /// 頂点構築の並列性レベル (スレッド数)。
        /// </summary>
        int concurrencyLevel = 10;

        /// <summary>
        /// 頂点構築タスク。
        /// </summary>
        BuildVertexTask buildVertexTask;

        /// <summary>
        /// メッシュ名の構築で利用する文字列ビルダ。
        /// </summary>
        StringBuilder meshNameBuilder = new StringBuilder(18);

        /// <summary>
        /// フレーム毎のバッファ反映数。
        /// </summary>
        int updateBufferCountPerFrame = 16;

        /// <summary>
        /// 頂点の総数を取得します。
        /// </summary>
        public int TotalVertexCount { get; private set; }

        /// <summary>
        /// インデックスの総数を取得します。
        /// </summary>
        public int TotalIndexCount { get; private set; }

        /// <summary>
        /// 処理全体を通じて最も大きな頂点バッファのサイズを取得します。
        /// </summary>
        public int MaxVertexCount { get; private set; }

        /// <summary>
        /// 処理全体を通じて最も大きなインデックス バッファのサイズを取得します。
        /// </summary>
        public int MaxIndexCount { get; private set; }

        /// <summary>
        /// チャンク メッシュの数を取得します。
        /// </summary>
        public int MeshCount { get; private set; }

        /// <summary>
        /// 頂点ビルダの総数を取得します。
        /// </summary>
        public int TotalVertexBuilderCount
        {
            get { return vertexBuilderPool.TotalObjectCount; }
        }

        /// <summary>
        /// 未使用の頂点ビルダの数を取得します。
        /// </summary>
        public int FreeVertexBuilderCount
        {
            get { return vertexBuilderPool.Count; }
        }

        public ChunkMeshManager(ChunkManager chunkManager, int concurrencyLevel, int updateBufferCountPerFrame)
        {
            if (chunkManager == null) throw new ArgumentNullException("chunkManager");

            this.chunkManager = chunkManager;
            this.concurrencyLevel = concurrencyLevel;
            this.updateBufferCountPerFrame = updateBufferCountPerFrame;

            buildVertexRequestPool = new ConcurrentPool<BuildVertexRequest>(() => { return new BuildVertexRequest(); });
            buildVertexRequests = new ConcurrentPriorityQueue<BuildVertexRequest>(new BuildVertexRequestComparer());
            updateBufferRequests = new ConcurrentPriorityQueue<UpdateBufferRequest>(new UpdateBufferRequestComparer());
            vertexBuilderPool = new ConcurrentPool<ChunkVertexBuilder>(() => { return new ChunkVertexBuilder(this); })
            {
                MaxCapacity = concurrencyLevel
            };
            buildVertexTask = new BuildVertexTask(this, OnBuildVertexTaskCallback);

            ChunkSize = chunkManager.ChunkSize;

            MeshSegments = new IntVector3
            {
                X = ChunkSize.X / MeshSize.X,
                Y = ChunkSize.Y / MeshSize.Y,
                Z = ChunkSize.Z / MeshSize.Z
            };

            var halfMeshSize = MeshSize;
            halfMeshSize.X /= 2;
            halfMeshSize.Y /= 2;
            halfMeshSize.Z /= 2;
            MeshOffset = halfMeshSize.ToVector3();
        }

        // メッシュ更新は２フェーズ。
        // 1. 非同期な頂点構築
        // 2. 構築された頂点による同期バッファ更新。
        //
        // 恐らく、バッファ更新は GPU との同期が発生するため、
        // 非同期ではなくゲーム スレッドで実行すべきであろうと思われる。

        public void RequestUpdateMesh(Chunk chunk, ChunkMeshUpdatePriority priority)
        {
            if (chunk == null) throw new ArgumentNullException("chunk");

            var request = buildVertexRequestPool.Borrow();
            request.Initialize(chunk, priority);

            buildVertexRequests.Enqueue(request);
        }

        void RequestUpdateBuffer(ChunkVertexBuilder vertexBuilder, ChunkMeshUpdatePriority priority)
        {
            for (int z = 0; z < MeshSegments.Z; z++)
            {
                for (int y = 0; y < MeshSegments.Y; y++)
                {
                    for (int x = 0; x < MeshSegments.X; x++)
                    {
                        var opaqueRequest = new UpdateBufferRequest
                        {
                            VertexBuilder = vertexBuilder,
                            Segment = new IntVector3(x, y, z),
                            Translucece = false,
                            Priority = priority,
                            RequestTime = TimeSpan.FromTicks(Environment.TickCount)
                        };
                        updateBufferRequests.Enqueue(opaqueRequest);

                        var translucentRequest = new UpdateBufferRequest
                        {
                            VertexBuilder = vertexBuilder,
                            Segment = new IntVector3(x, y, z),
                            Translucece = true,
                            Priority = priority,
                            RequestTime = TimeSpan.FromTicks(Environment.TickCount)
                        };
                        updateBufferRequests.Enqueue(translucentRequest);
                    }
                }
            }
        }

        void ProcessBuildVertexRequests()
        {
            ChunkVertexBuilder vertexBuilder;
            while (vertexBuilderPool.TryBorrow(out vertexBuilder))
            {
                BuildVertexRequest request;
                if (!buildVertexRequests.TryDequeue(out request))
                {
                    vertexBuilderPool.Return(vertexBuilder);
                    break;
                }

                request.AttachVertexBuilder(vertexBuilder);

                ThreadPool.QueueUserWorkItem(buildVertexTask.WaitCallbackMethod, request);
            }
        }

        void OnBuildVertexTaskCallback(BuildVertexRequest request)
        {
            RequestUpdateBuffer(request.VertexBuilder, request.Priority);

            request.Clear();
            buildVertexRequestPool.Return(request);
        }

        public void Update()
        {
            ProcessBuildVertexRequests();
            ProcessUpdateBufferRequests();
        }

        void ProcessUpdateBufferRequests()
        {
            int updateBufferCount = 0;

            UpdateBufferRequest request;
            while (updateBufferCount < updateBufferCountPerFrame && updateBufferRequests.TryDequeue(out request))
            {
                var verticesBuilder = request.VertexBuilder;
                var chunk = verticesBuilder.Chunk;

                // チャンクの非アクティブ化は、このメソッドと同じスレッドで処理される。
                // このため、Active プロパティが示す値は、このメソッドで保証される。
                if (!chunk.Active)
                    continue;

                var segment = request.Segment;
                var translucence = request.Translucece;

                // クローズ中は頂点バッファ反映をスキップ。
                if (!chunkManager.Closing)
                {
                    if (UpdateMeshSegmentBuffer(verticesBuilder, segment.X, segment.Y, segment.Z, translucence))
                    {
                        // バッファに変更があったならばチャンクのノードを更新。
                        chunk.Node.Update(false);

                        updateBufferCount++;
                    }
                }

                // バッファを反映済みとしてマーク。
                var vertices = verticesBuilder.GetVertices(segment.X, segment.Y, segment.Z, translucence);
                vertices.Consumed = true;

                // 関連する全てのバッファへ反映したならば頂点ビルダを解放。
                if (verticesBuilder.ConsumedAll())
                {
                    verticesBuilder.Clear();
                    vertexBuilderPool.Return(verticesBuilder);
                }
            }
        }

        bool UpdateMeshSegmentBuffer(ChunkVertexBuilder verticesBuilder, int segmentX, int segmentY, int segmentZ, bool translucence)
        {
            var chunk = verticesBuilder.Chunk;

            // メッシュに設定するワールド座標。
            // メッシュの中心をメッシュの位置とする。
            var position = new Vector3
            {
                X = chunk.Position.X * chunkManager.PartitionSize.X + segmentX * MeshSize.X + MeshOffset.X,
                Y = chunk.Position.Y * chunkManager.PartitionSize.Y + segmentY * MeshSize.Y + MeshOffset.Y,
                Z = chunk.Position.Z * chunkManager.PartitionSize.Z + segmentZ * MeshSize.Z + MeshOffset.Z
            };

            // メッシュに設定するワールド行列。
            Matrix world;
            Matrix.CreateTranslation(ref position, out world);

            // メッシュに設定するエフェクト。
            var effect = chunk.Region.ChunkEffect;

            var vertices = verticesBuilder.GetVertices(segmentX, segmentY, segmentZ, translucence);

            var mesh = chunk.GetMesh(segmentX, segmentY, segmentZ, translucence);
            if (mesh != null)
            {
                TotalVertexCount -= mesh.VertexCount;
                TotalIndexCount -= mesh.IndexCount;
            }

            bool updated;
            if (vertices.VertexCount == 0 || vertices.IndexCount == 0)
            {
                if (mesh != null)
                {
                    chunk.SetMesh(segmentX, segmentY, segmentZ, translucence, null);
                    updated = true;
                }
                else
                {
                    // 更新不要。
                    updated = false;
                }
            }
            else
            {
                if (mesh == null)
                {
                    mesh = CreateMesh(effect, false, segmentX, segmentY, segmentZ);
                    chunk.SetMesh(segmentX, segmentY, segmentZ, translucence, mesh);
                }

                mesh.PositionWorld = position;
                mesh.World = world;
                vertices.Populate(mesh);

                TotalVertexCount += mesh.VertexCount;
                TotalIndexCount += mesh.IndexCount;
                MaxVertexCount = Math.Max(MaxVertexCount, mesh.VertexCount);
                MaxIndexCount = Math.Max(MaxIndexCount, mesh.IndexCount);

                updated = true;
            }

            return updated;
        }

        /// <summary>
        /// チャンク メッシュを生成します。
        /// </summary>
        /// <param name="effect">チャンクのエフェクト。</param>
        /// <param name="translucent">
        /// true (半透明の場合)、false (それ以外の場合)。
        /// </param>
        /// <returns>チャンク メッシュ。</returns>
        ChunkMesh CreateMesh(ChunkEffect effect, bool translucent, int segmentX, int segmentY, int segmentZ)
        {
            meshNameBuilder.Length = 0;
            meshNameBuilder.Append((translucent) ? "TranslucentMesh" : "OpaqueMesh");
            meshNameBuilder.Append('_').AppendNumber(segmentX);
            meshNameBuilder.Append('_').AppendNumber(segmentY);
            meshNameBuilder.Append('_').AppendNumber(segmentZ);

            var name = meshNameBuilder.ToString();

            var mesh = new ChunkMesh(name, effect);
            mesh.Translucent = translucent;

            MeshCount++;

            return mesh;
        }

        internal void DisposeMesh(ChunkMesh chunkMesh)
        {
            TotalVertexCount -= chunkMesh.VertexCount;
            TotalIndexCount -= chunkMesh.IndexCount;

            chunkMesh.Dispose();

            MeshCount--;
        }
    }
}
