#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    /// <summary>
    /// パーティクルを表示するクラスです。
    /// XNA サンプルの『パーティクル 3D』を参考に改変しています。
    /// </summary>
    public sealed class ParticleSystem : IDisposable
    {
        #region ParticleEffect

        internal sealed class ParticleEffect : Effect
        {
            EffectParameter view;
            EffectParameter projection;
            EffectParameter viewportScale;
            EffectParameter currentTime;

            internal Matrix View
            {
                get { return view.GetValueMatrix(); }
                set { view.SetValue(value); }
            }

            internal Matrix Projection
            {
                get { return projection.GetValueMatrix(); }
                set { projection.SetValue(value); }
            }

            internal Vector2 ViewportScale
            {
                get { return viewportScale.GetValueVector2(); }
                set { viewportScale.SetValue(value); }
            }

            internal float CurrentTime
            {
                get { return currentTime.GetValueSingle(); }
                set { currentTime.SetValue(value); }
            }

            internal ParticleEffect(Effect cloneSource)
                : base(cloneSource)
            {
                view = Parameters["View"];
                projection = Parameters["Projection"];
                viewportScale = Parameters["ViewportScale"];
                currentTime = Parameters["CurrentTime"];
            }

            internal void Initialize(ParticleSettings settings, Texture2D texture)
            {
                Parameters["Duration"].SetValue((float) settings.Duration.TotalSeconds);
                Parameters["DurationRandomness"].SetValue(settings.DurationRandomness);
                Parameters["Gravity"].SetValue(settings.Gravity);
                Parameters["EndVelocity"].SetValue(settings.EndVelocity);
                Parameters["MinColor"].SetValue(settings.MinColor.ToVector4());
                Parameters["MaxColor"].SetValue(settings.MaxColor.ToVector4());
                Parameters["RotateSpeed"].SetValue(new Vector2(settings.MinRotateSpeed, settings.MaxRotateSpeed));
                Parameters["StartSize"].SetValue(new Vector2(settings.MinStartSize, settings.MaxStartSize));
                Parameters["EndSize"].SetValue(new Vector2(settings.MinEndSize, settings.MaxEndSize));
                Parameters["Texture"].SetValue(texture);
            }
        }

        #endregion

        #region ParticleVertex

        /// <summary>
        /// ポイント スプライト パーティクルを描画するためのカスタム頂点構造体。
        /// </summary>
        struct ParticleVertex
        {
            // この頂点がパーティクルのどのコーナーに相当するかを保存します。
            public Short2 Corner;

            // パーティクルの開始位置を格納します。
            public Vector3 Position;

            // パーティクルの開始速度を格納します。
            public Vector3 Velocity;

            // 各パーティクルをわずかに異なる外観にするための 4 つのランダム値。
            public Color Random;

            // このパーティクルの作成時刻 (秒単位)。
            public float Time;

            // この頂点構造体のレイアウトを記述します。
            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Short2, VertexElementUsage.Position, 0),
                new VertexElement(4, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
                new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(28, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(32, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0)
            );

            // この頂点構造体のサイズを記述します。
            public const int SizeInBytes = 36;
        }

        #endregion

        static Random random = new Random();

        ParticleSettings settings;

        ParticleEffect particleEffect;

        ParticleVertex[] particles;

        DynamicVertexBuffer vertexBuffer;

        IndexBuffer indexBuffer;

        int firstActiveParticle;
        
        int firstNewParticle;
        
        int firstFreeParticle;
        
        int firstRetiredParticle;

        float currentTime;

        int drawCounter;

        GraphicsDevice graphicsDevice;

        public bool Enabled { get; set; }

        public string Name { get; private set; }

        public ParticleSystem(ParticleSettings settings, Effect effect, Texture2D texture)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (effect == null) throw new ArgumentNullException("effect");
            if (texture == null) throw new ArgumentNullException("texture");

            Enabled = true;

            this.settings = settings;

            //----------------------------------------------------------------
            // システム名

            Name = settings.Name;

            //----------------------------------------------------------------
            // エフェクト

            particleEffect = new ParticleEffect(effect);
            particleEffect.Initialize(settings, texture);

            graphicsDevice = particleEffect.GraphicsDevice;

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
                graphicsDevice, ParticleVertex.VertexDeclaration, settings.MaxParticles * 4, BufferUsage.WriteOnly);

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

            indexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        public void AddParticle(Vector3 position, Vector3 velocity)
        {
            var nextFreeParticle = firstFreeParticle + 1;

            if (nextFreeParticle >= settings.MaxParticles)
                nextFreeParticle = 0;

            if (nextFreeParticle == firstRetiredParticle)
                return;

            velocity *= settings.EmitterVelocitySensitivity;

            var horizontalVelocity = MathHelper.Lerp(
                settings.MinHorizontalVelocity, settings.MaxHorizontalVelocity, (float) random.NextDouble());

            var horizontalAngle = random.NextDouble() * MathHelper.TwoPi;

            velocity.X += horizontalVelocity * (float) Math.Cos(horizontalAngle);
            velocity.Z += horizontalVelocity * (float) Math.Sin(horizontalAngle);

            velocity.Y += MathHelper.Lerp(
                settings.MinVerticalVelocity, settings.MaxVerticalVelocity, (float) random.NextDouble());

            var randomValues = new Color((byte) random.Next(255), (byte) random.Next(255), (byte) random.Next(255), (byte) random.Next(255));

            for (int i = 0; i < 4; i++)
            {
                particles[firstFreeParticle * 4 + i].Position = position;
                particles[firstFreeParticle * 4 + i].Velocity = velocity;
                particles[firstFreeParticle * 4 + i].Random = randomValues;
                particles[firstFreeParticle * 4 + i].Time = currentTime;
            }

            firstFreeParticle = nextFreeParticle;
        }

        public void Draw(GameTime gameTime, ICamera camera)
        {
            if (gameTime == null) throw new ArgumentNullException("gameTime");
            if (camera == null) throw new ArgumentNullException("camera");
            if (!Enabled) return;

            //------------------------------------------------------------
            // パーティクルの状態更新

            currentTime += (float) gameTime.ElapsedGameTime.TotalSeconds;

            RetireActiveParticles();
            FreeRetiredParticles();

            if (firstActiveParticle == firstFreeParticle)
                currentTime = 0;

            if (firstRetiredParticle == firstActiveParticle)
                drawCounter = 0;

            //------------------------------------------------------------
            // 描画処理の開始

            if (vertexBuffer.IsContentLost)
                vertexBuffer.SetData(particles);

            if (firstNewParticle != firstFreeParticle)
                AddNewParticlesToVertexBuffer();

            if (firstActiveParticle != firstFreeParticle)
            {
                //------------------------------------------------------------
                // エフェクト

                particleEffect.View = camera.View.Matrix;
                particleEffect.Projection = camera.Projection.Matrix;
                particleEffect.ViewportScale = new Vector2(0.5f / graphicsDevice.Viewport.AspectRatio, -0.5f);
                particleEffect.CurrentTime = currentTime;

                //------------------------------------------------------------
                // 描画

                graphicsDevice.BlendState = settings.BlendState;
                graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                graphicsDevice.SetVertexBuffer(vertexBuffer);
                graphicsDevice.Indices = indexBuffer;

                foreach (var pass in particleEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    if (firstActiveParticle < firstFreeParticle)
                    {
                        graphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList, 0,
                            firstActiveParticle * 4, (firstFreeParticle - firstActiveParticle) * 4,
                            firstActiveParticle * 6, (firstFreeParticle - firstActiveParticle) * 2);
                    }
                    else
                    {
                        // 折り返しが必要な場合。
                        graphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList, 0,
                            firstActiveParticle * 4, (settings.MaxParticles - firstActiveParticle) * 4,
                            firstActiveParticle * 6, (settings.MaxParticles - firstActiveParticle) * 2);

                        if (0 < firstFreeParticle)
                        {
                            graphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleList, 0,
                                0, firstFreeParticle * 4,
                                0, firstFreeParticle * 2);
                        }
                    }
                }

                graphicsDevice.DepthStencilState = DepthStencilState.Default;
            }

            drawCounter++;
        }

        void RetireActiveParticles()
        {
            var particleDuration = (float) settings.Duration.TotalSeconds;

            while (firstActiveParticle != firstNewParticle)
            {
                var particleAge = currentTime - particles[firstActiveParticle * 4].Time;

                if (particleAge < particleDuration)
                    break;

                particles[firstActiveParticle * 4].Time = drawCounter;

                firstActiveParticle++;

                if (firstActiveParticle >= settings.MaxParticles)
                    firstActiveParticle = 0;
            }
        }

        void FreeRetiredParticles()
        {
            while (firstRetiredParticle != firstActiveParticle)
            {
                var age = drawCounter - (int) particles[firstRetiredParticle * 4].Time;

                if (age < 3)
                    break;

                firstRetiredParticle++;

                if (firstRetiredParticle >= settings.MaxParticles)
                    firstRetiredParticle = 0;
            }
        }

        void AddNewParticlesToVertexBuffer()
        {
            var stride = ParticleVertex.SizeInBytes;

            if (firstNewParticle < firstFreeParticle)
            {
                vertexBuffer.SetData(
                    firstNewParticle * stride * 4, particles,
                    firstNewParticle * 4,
                    (firstFreeParticle - firstNewParticle) * 4,
                    stride, SetDataOptions.NoOverwrite);
            }
            else
            {
                // 折り返しが必要な場合。
                vertexBuffer.SetData(
                    firstNewParticle * stride * 4, particles,
                    firstNewParticle * 4,
                    (settings.MaxParticles - firstNewParticle) * 4,
                    stride, SetDataOptions.NoOverwrite);

                if (0 < firstFreeParticle)
                    vertexBuffer.SetData(0, particles, 0, firstFreeParticle * 4, stride, SetDataOptions.NoOverwrite);
            }

            firstNewParticle = firstFreeParticle;
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~ParticleSystem()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                particleEffect.Dispose();
                vertexBuffer.Dispose();
                indexBuffer.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
