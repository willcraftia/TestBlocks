#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    /// <summary>
    /// パーティクルを表示するメイン コンポーネント。
    /// </summary>
    public abstract class ParticleSystem
    {
        // settings クラスが、このパーティクル システムの外観とアニメーションを制御します。
        ParticleSettings settings = new ParticleSettings();

        // ポイント スプライト パーティクルを描画するためのカスタム エフェクト。頂点シェーダー内の
        // すべてのパーティクル アニメーションを計算します。パーティクル単位の CPU 処理は不要。
        Effect particleEffect;

        // 頻繁に変更されるエフェクト パラメーターにアクセスするためのショートカット。
        EffectParameter effectViewParameter;
        EffectParameter effectProjectionParameter;
        EffectParameter effectViewportScaleParameter;
        EffectParameter effectTimeParameter;

        // 巡回待ち行列として扱われるパーティクルの配列。
        ParticleVertex[] particles;

        // パーティクルを保持する頂点バッファー。パーティクル配列と同じデータを含むが、
        // GPU がアクセスできる場所へコピーされます。
        DynamicVertexBuffer vertexBuffer;

        // 頂点宣言には ParticleVertex 構造体の形式が記述されます。
        IndexBuffer indexBuffer;

        // パーティクル配列と頂点バッファーは、巡回待ち行列として扱われます。
        // 最初は、使用されるパーティクルがないため、配列のコンテンツ全体はフリーです。
        // 新しいパーティクルの作成時には、配列の先頭から割り当てられます。複数のパーティクルが
        // 作成される場合は、常に配列要素の連続したブロックに格納されます。すべてのパーティクルが
        // 同じ時間だけ存続するため、古いパーティクルは常にこのアクティブなパーティクル領域の
        // 先頭から順番に削除されます。そのため、アクティブな領域とフリーの領域が混ざり合うことは
        // ありません。待ち行列は巡回するため、アクティブなパーティクル領域が配列の末尾から
        // 先頭に折り返すことがあります。待ち行列は剰余演算を使用して、
        // これらのケースに対処します。たとえば、待ち行列に次の 4 つのエントリが存在する
        // 場合を考えてみます。
        //
        //      0
        //      1 - first active particle
        // 
        //      1 - 最初のアクティブ パーティクル
        //      2 
        //      3 - first free particle
        // 
        //      3 - 最初のフリー パーティクル
        //
        // この場合、パーティクル 1 とパーティクル 2 がアクティブで、パーティクル 3 と 
        // パーティクル 0 がフリーです。
        //
        //      0
        //      1 - first free particle
        // 
        //      1 - 最初のフリー パーティクル
        //      2 
        //      3 - first active particle
        // 
        //      3 - 最初のアクティブ パーティクル
        //
        // この場合、3 と 0 がアクティブで、1 と 2 がフリーです。
        //
        // ただし、実際には、このように単純ではありません。
        //
        // 新しいパーティクルの作成時には、それらをパーティクルのマネージ配列に追加します。
        // また、この新しいデータを GPU の頂点バッファーにコピーする必要もあります。しかし、
        // 新しいデータを頂点バッファーに設定するのはコストのかかる操作なので、すぐには
        // 行いません。複数のパーティクルを単一のフレームに追加する場合、それらを最初に
        // マネージ配列だけに格納し、後に 1 回の呼び出しですべてを GPU にアップロード
        // するようにすれば処理が速くなります。そのため、待ち行列には、マネージ配列に
        // 追加されたのに頂点バッファーにはアップロードされていない新しいパーティクルを
        // 格納する領域も必要となります。
        //
        // 古いパーティクルを退去させるときにも別の問題が発生します。CPU と GPU は
        // 非同期に実行されているので、GPU が前のフレームの描画中でも、CPU が
        // 次のフレームを処理することが多くあります。これによって、古いパーティクルの
        // 退去後すぐに新しいパーティクルを上書きする場合、GPU が頂点バッファーの古いデータを
        // 使用して描画しているときに、CPU が頂点バッファーの内容を変更しようとしてしまうため、
        // 同期上の問題が発生する可能性があります。通常、グラフィック ドライバーは
        // VertexBuffer.SetData の呼び出しの内側で GPU が描画を終了するまで
        // 待機することで、この問題に対処します。しかし、新しいパーティクルの追加時に
        // 待機時間を浪費するのは好ましくありません。この遅延を回避するために、
        // 頂点バッファーへの書き込み時に SetDataOptions.NoOverwrite フラグを
        // 指定することができます。これは、基本的に "CPU が使用中のデータへの上書きを
        // 禁止するので、処理を先に進めて、すぐにバッファーを更新できる" ことを意味します。
        // この条件を維持するためには、頂点を描画後すぐに再利用しないようにする必要が
        // あります。
        //
        // そのため全体として、待ち行列には以下の 4 種類の領域が存在することになります。
        //
        // firstActiveParticle と firstNewParticle の間の頂点がアクティブに
        // 描画中であり、パーティクルのマネージ配列と GPU の頂点バッファーの
        // 両方に存在する場合。
        //
        // firstNewParticle と firstFreeParticle の間の頂点が新たに作成され、
        // パーティクルのマネージ配列にのみ存在する場合。これらは、次の描画の
        // 呼び出しが開始されるときに GPU にアップロードされる必要があります。
        //
        // firstFreeParticle と firstRetiredParticle の間の頂点がフリーで、割り当てを
        // 待機している場合。
        //
        // firstRetiredParticle と firstActiveParticle の間の頂点は以降描画される
        // ことがないが、まだ GPU が使用している可能性がある場合。これらは、
        // それ以降のいくつかのフレームでも保持される必要があります。その後、
        // 再割り当てが可能となります。

        int firstActiveParticle;
        int firstNewParticle;
        int firstFreeParticle;
        int firstRetiredParticle;

        // 現在の時刻を秒単位で格納。
        float currentTime;

        // 描画が呼び出された回数をカウントします。安全に古いパーティクルを退去させて、
        // フリー リストに戻すことができる時期を検知するために使用されます。
        int drawCounter;

        // 共有の乱数ジェネレーター。
        static Random random = new Random();

        protected GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// コンストラクター。
        /// </summary>
        protected ParticleSystem(Effect effect, Texture2D texture)
        {
            if (effect == null) throw new ArgumentNullException("effect");
            if (texture == null) throw new ArgumentNullException("texture");

            InitializeSettings(settings);

            // 不変パラメータの事前設定のために複製。
            particleEffect = effect.Clone();

            GraphicsDevice = particleEffect.GraphicsDevice;

            //----------------------------------------------------------------
            // エフェクト

            var parameters = particleEffect.Parameters;

            effectViewParameter = parameters["View"];
            effectProjectionParameter = parameters["Projection"];
            effectViewportScaleParameter = parameters["ViewportScale"];
            effectTimeParameter = parameters["CurrentTime"];

            // 不変パラメータの事前設定。
            parameters["Duration"].SetValue((float) settings.Duration.TotalSeconds);
            parameters["DurationRandomness"].SetValue(settings.DurationRandomness);
            parameters["Gravity"].SetValue(settings.Gravity);
            parameters["EndVelocity"].SetValue(settings.EndVelocity);
            parameters["MinColor"].SetValue(settings.MinColor.ToVector4());
            parameters["MaxColor"].SetValue(settings.MaxColor.ToVector4());
            parameters["RotateSpeed"].SetValue(new Vector2(settings.MinRotateSpeed, settings.MaxRotateSpeed));
            parameters["StartSize"].SetValue(new Vector2(settings.MinStartSize, settings.MaxStartSize));
            parameters["EndSize"].SetValue(new Vector2(settings.MinEndSize, settings.MaxEndSize));
            parameters["Texture"].SetValue(texture);

            //----------------------------------------------------------------
            // パーティクル

            particles = new ParticleVertex[settings.MaxParticles * 4];

            for (int i = 0; i < settings.MaxParticles; i++)
            {
                particles[i * 4 + 0].Corner = new Short2(-1, -1);
                particles[i * 4 + 1].Corner = new Short2(1, -1);
                particles[i * 4 + 2].Corner = new Short2(1, 1);
                particles[i * 4 + 3].Corner = new Short2(-1, 1);
            }

            //----------------------------------------------------------------
            // 頂点バッファ

            vertexBuffer = new DynamicVertexBuffer(
                GraphicsDevice, ParticleVertex.VertexDeclaration, settings.MaxParticles * 4, BufferUsage.WriteOnly);

            //----------------------------------------------------------------
            // インデックス バッファ

            var indices = new ushort[settings.MaxParticles * 6];
            for (int i = 0; i < settings.MaxParticles; i++)
            {
                indices[i * 6 + 0] = (ushort) (i * 4 + 0);
                indices[i * 6 + 1] = (ushort) (i * 4 + 1);
                indices[i * 6 + 2] = (ushort) (i * 4 + 2);

                indices[i * 6 + 3] = (ushort) (i * 4 + 0);
                indices[i * 6 + 4] = (ushort) (i * 4 + 2);
                indices[i * 6 + 5] = (ushort) (i * 4 + 3);
            }

            indexBuffer = new IndexBuffer(GraphicsDevice, typeof(ushort), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        /// <summary>
        /// パーティクル システムの派生クラスがこのメソッドをオーバーライドし、
        /// 調整可能な設定を初期化します。
        /// </summary>
        protected abstract void InitializeSettings(ParticleSettings settings);

        /// <summary>
        /// パーティクル システムを更新します。
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (gameTime == null) throw new ArgumentNullException("gameTime");

            currentTime += (float) gameTime.ElapsedGameTime.TotalSeconds;

            RetireActiveParticles();
            FreeRetiredParticles();

            // タイマーの値を常に増加させる場合、最終的に浮動小数点精度から外れて、
            // その時点でパーティクルのレンダリングが不正になります。これを
            // 簡単に防止するには、描画されるパーティクルがないときには、時刻の値が
            // 重要でないことに気付くことが重要です。つまり、アクティブな待ち行列が空のときに、
            // 時刻の値をゼロにリセットできます。

            if (firstActiveParticle == firstFreeParticle)
                currentTime = 0;
            
            if (firstRetiredParticle == firstActiveParticle)
                drawCounter = 0;
        }

        /// <summary>
        /// アクティブなパーティクルの存続期間がいつ終了するかをチェックするヘルパー。
        /// 古いパーティクルを、待ち行列のアクティブ領域から退去セクションに
        /// 移動します。
        /// </summary>
        void RetireActiveParticles()
        {
            var particleDuration = (float) settings.Duration.TotalSeconds;

            while (firstActiveParticle != firstNewParticle)
            {
                // パーティクルを退去させるかどうかのチェック。
                // アクティブなパーティクルのインデックスは 4 で乗算します。
                // これは各パーティクルが 4 つの頂点によって形成される四角形だからです。
                var particleAge = currentTime - particles[firstActiveParticle * 4].Time;

                if (particleAge < particleDuration)
                    break;

                // パーティクルの退去時刻を格納します。
                particles[firstActiveParticle * 4].Time = drawCounter;

                // パーティクルをアクティブな待ち行列から退去済み待ち行列に移動します。
                firstActiveParticle++;

                if (firstActiveParticle >= settings.MaxParticles)
                    firstActiveParticle = 0;
            }
        }

        /// <summary>
        /// 退去済みパーティクルが十分に保持され、以降 GPU に使用されないことを
        /// チェックするヘルパー。古いパーティクルを、待ち行列の退去済み領域から
        /// フリー セクションに移動します。
        /// </summary>
        void FreeRetiredParticles()
        {
            while (firstRetiredParticle != firstActiveParticle)
            {
                // GPU での終了を判断できるだけ、パーティクルの未使用期間が
                // 経過したかどうか。
                // アクティブなパーティクルのインデックスは 4 で乗算します。
                // これは各パーティクルが 4 つの頂点によって形成される四角形だからです。
                var age = drawCounter - (int) particles[firstRetiredParticle * 4].Time;

                // GPU には CPU より遅れて 2 フレームを超える差は生じないと想定します。
                // 通常の動作をしないドライバーなどによって GPU がこれよりも遅れる場合の
                // 安全策として、1 を加えておきます。
                if (age < 3)
                    break;

                // パーティクルを退去済み待ち行列からフリーの待ち行列に移動します。
                firstRetiredParticle++;

                if (firstRetiredParticle >= settings.MaxParticles)
                    firstRetiredParticle = 0;
            }
        }

        /// <summary>
        /// パーティクル システムを描画します。
        /// </summary>
        public void Draw(ICamera camera)
        {
            // グラフィック デバイスが失われた場合、頂点バッファーのコンテンツを復元します。
            if (vertexBuffer.IsContentLost)
                vertexBuffer.SetData(particles);

            // 新しく追加された待ち行列に待機状態のパーティクルがある場合、
            // それらを描画の準備が整っている GPU にアップロードします。
            if (firstNewParticle != firstFreeParticle)
                AddNewParticlesToVertexBuffer();

            // アクティブなパーティクルが存在する場合、それらを描画します。
            if (firstActiveParticle != firstFreeParticle)
            {
                GraphicsDevice.BlendState = settings.BlendState;
                GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

                // このパーティクル システムの描画に使用するカメラの
                // ビュー行列と射影行列を設定します。
                effectViewParameter.SetValue(camera.View.Matrix);
                effectProjectionParameter.SetValue(camera.Projection.Matrix);

                // ビューポートのサイズを示すエフェクト パラメーターを設定します。これは、
                // パーティクル サイズをスクリーン空間のポイント スプライト サイズに
                // 変換するために必要となります。
                effectViewportScaleParameter.SetValue(new Vector2(0.5f / GraphicsDevice.Viewport.AspectRatio, -0.5f));

                // 現在の時刻を示すエフェクト パラメーターを設定します。すべての頂点
                // シェーダー パーティクル アニメーションは、この値で調整されます。
                effectTimeParameter.SetValue(currentTime);

                // パーティクルの頂点バッファーと頂点宣言を設定します。
                GraphicsDevice.SetVertexBuffer(vertexBuffer);
                GraphicsDevice.Indices = indexBuffer;

                // パーティクル エフェクトをアクティブ化します。
                foreach (var pass in particleEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    if (firstActiveParticle < firstFreeParticle)
                    {
                        // アクティブなパーティクルがすべて 1 つの連続した範囲にある場合は、
                        // それらすべてを 1 回の呼び出しで描画できます。
                        GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList, 0,
                            firstActiveParticle * 4, (firstFreeParticle - firstActiveParticle) * 4,
                            firstActiveParticle * 6, (firstFreeParticle - firstActiveParticle) * 2);
                    }
                    else
                    {
                        // アクティブなパーティクルの範囲が待ち行列の末尾から先頭まで
                        // 折り返している場合は、それらを 2 回の描画の呼び出しに分割する必要があります。
                        GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList, 0,
                            firstActiveParticle * 4, (settings.MaxParticles - firstActiveParticle) * 4,
                            firstActiveParticle * 6, (settings.MaxParticles - firstActiveParticle) * 2);

                        if (0 < firstFreeParticle)
                        {
                            GraphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleList, 0,
                                0, firstFreeParticle * 4,
                                0, firstFreeParticle * 2);
                        }
                    }
                }

                // 後続の描画で混乱が生じないように、通常とは異なる変更済み
                // レンダリング ステートの組をリセットします。
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }

            drawCounter++;
        }

        /// <summary>
        /// 新しいパーティクルをマネージ配列から GPU 頂点バッファーに
        /// アップロードするヘルパー。
        /// </summary>
        void AddNewParticlesToVertexBuffer()
        {
            var stride = ParticleVertex.SizeInBytes;

            if (firstNewParticle < firstFreeParticle)
            {
                // 新しいパーティクルがすべて 1 つの連続した範囲にある場合は、
                // それらすべてを 1 回の呼び出しでアップロードできます。
                vertexBuffer.SetData(
                    firstNewParticle * stride * 4, particles,
                    firstNewParticle * 4,
                    (firstFreeParticle - firstNewParticle) * 4,
                    stride, SetDataOptions.NoOverwrite);
            }
            else
            {
                // 新しいパーティクルの範囲が待ち行列の末尾から先頭まで折り返している
                // 場合は、それらを 2 回のアップロードの呼び出しに分割する必要があります。
                vertexBuffer.SetData(
                    firstNewParticle * stride * 4, particles,
                    firstNewParticle * 4,
                    (settings.MaxParticles - firstNewParticle) * 4,
                    stride, SetDataOptions.NoOverwrite);

                if (0 < firstFreeParticle)
                    vertexBuffer.SetData(0, particles, 0, firstFreeParticle * 4, stride, SetDataOptions.NoOverwrite);
            }

            // アップロードしたパーティクルを新しい待ち行列からアクティブな待ち行列に移動します。
            firstNewParticle = firstFreeParticle;
        }

        /// <summary>
        /// 新しいパーティクルをシステムに追加します。
        /// </summary>
        public void AddParticle(Vector3 position, Vector3 velocity)
        {
            // 巡回待ち行列のどこに新しいパーティクルを割り当てるかを判別します。
            var nextFreeParticle = firstFreeParticle + 1;

            if (nextFreeParticle >= settings.MaxParticles)
                nextFreeParticle = 0;

            // フリーのパーティクルがない場合は、あきらめます。
            if (nextFreeParticle == firstRetiredParticle)
                return;

            // このパーティクル システムに与える影響の大きさに
            // 基づいて入力速度を調整します。
            velocity *= settings.EmitterVelocitySensitivity;

            // 水平方向の速度にランダムな量を加算します。
            var horizontalVelocity = MathHelper.Lerp(
                settings.MinHorizontalVelocity, settings.MaxHorizontalVelocity, (float) random.NextDouble());

            var horizontalAngle = random.NextDouble() * MathHelper.TwoPi;

            velocity.X += horizontalVelocity * (float) Math.Cos(horizontalAngle);
            velocity.Z += horizontalVelocity * (float) Math.Sin(horizontalAngle);

            // 垂直方向の速度にランダムな量を加算します。
            velocity.Y += MathHelper.Lerp(
                settings.MinVerticalVelocity, settings.MaxVerticalVelocity, (float) random.NextDouble());

            // 4 つの制御値をランダムに選択します。頂点シェーダーは、これらを使用して、
            // 各パーティクルに異なるサイズ、回転、およびカラーを与えます。
            var randomValues = new Color((byte) random.Next(255), (byte) random.Next(255), (byte) random.Next(255), (byte) random.Next(255));

            // パーティクルの頂点構造体に値を設定します。
            for (int i = 0; i < 4; i++)
            {
                particles[firstFreeParticle * 4 + i].Position = position;
                particles[firstFreeParticle * 4 + i].Velocity = velocity;
                particles[firstFreeParticle * 4 + i].Random = randomValues;
                particles[firstFreeParticle * 4 + i].Time = currentTime;
            }

            firstFreeParticle = nextFreeParticle;
        }
    }
}
