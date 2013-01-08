#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    // 『ゲーム エフェクト マニアックス』より。
    // 元のコードでは Y = 0 を最小としているため地平線 (視線の Y) 到達で雪が消滅するが、
    // ここでは最小 Y を変更できるように変更した。

    public sealed class Snow
    {
        #region SnowEffect

        sealed class SnowEffect
        {
            EffectParameter projection;

            EffectParameter color;

            EffectParameter texture;

            internal Effect Effect { get; private set; }

            internal Matrix Projection
            {
                get { return projection.GetValueMatrix(); }
                set { projection.SetValue(value); }
            }

            internal Vector4 Color
            {
                get { return color.GetValueVector4(); }
                set { color.SetValue(value); }
            }

            internal Texture2D Texture
            {
                get { return texture.GetValueTexture2D(); }
                set { texture.SetValue(value); }
            }

            internal SnowEffect(Effect effect)
            {
                Effect = effect;

                projection = effect.Parameters["Projection"];
                color = effect.Parameters["Color"];
                texture = effect.Parameters["Texture"];
            }

            internal void Apply()
            {
                Effect.CurrentTechnique.Passes[0].Apply();
            }
        }

        #endregion

        #region SnowSprite

        sealed class SnowSprite : IComparable<SnowSprite>
        {
            static readonly Random random = new Random();

            public readonly VertexPositionTexture[] Vertices = new VertexPositionTexture[4];

            Vector3 position;

            float size;

            float vy;

            float maxXZ;

            float minY;

            float maxY;

            float minSize;

            float maxSize;

            internal SnowSprite(float vy, float maxXZ, float minY, float maxY, float minSize, float maxSize)
            {
                this.vy = vy;
                this.maxXZ = maxXZ;
                this.minY = minY;
                this.maxY = maxY;
                this.minSize = minSize;
                this.maxSize = maxSize;

                var rangeY = maxY - minY;

                position = new Vector3
                {
                    X = NextFloatOffset() * maxXZ,
                    Y = NextFloat() * rangeY + minY,
                    Z = NextFloatOffset() * maxXZ
                };
                size = minSize + (maxSize - minSize) * NextFloat();

                Vertices[0].TextureCoordinate = new Vector2(0, 0);
                Vertices[1].TextureCoordinate = new Vector2(1, 0);
                Vertices[2].TextureCoordinate = new Vector2(0, 1);
                Vertices[3].TextureCoordinate = new Vector2(1, 1);
            }

            static float NextFloat()
            {
                return (float) random.NextDouble();
            }

            static float NextFloatOffset()
            {
                return (float) (random.NextDouble() - 0.5f);
            }

            internal void Move()
            {
                position.Y += vy;

                if (position.Y < minY)
                {
                    position = new Vector3
                    {
                        X = NextFloatOffset() * maxXZ,
                        Y = maxY,
                        Z = NextFloatOffset() * maxXZ
                    };
                    size = minSize + (maxSize - minSize) * NextFloat();
                }
            }

            internal void SetVertex(ref Matrix wview)
            {
                var v = position;
                Vector3.Transform(ref v, ref wview, out v);
                Vertices[0].Position = new Vector3(v.X - size, v.Y + size, v.Z);
                Vertices[1].Position = new Vector3(v.X + size, v.Y + size, v.Z);
                Vertices[2].Position = new Vector3(v.X - size, v.Y - size, v.Z);
                Vertices[3].Position = new Vector3(v.X + size, v.Y - size, v.Z);
            }

            internal void Draw(SnowEffect effect)
            {
                var graphicsDevice = effect.Effect.GraphicsDevice;

                // TODO: XYZ には着色の設定ができても良い気がする。
                effect.Color = new Vector4
                {
                    X = 1, Y = 1, Z = 1,
                    W = (float) Math.Sin(MathHelper.Pi * (maxY - position.Y) / maxY)
                };

                effect.Apply();

                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, Vertices, 0, 2);
            }

            #region IComparable

            public int CompareTo(SnowSprite other)
            {
                var z1 = Vertices[0].Position.Z;
                var z2 = other.Vertices[0].Position.Z;
                if (z1 == z2) return 0;

                return z1 < z2 ? -1 : 1;
            }

            #endregion
        }

        #endregion

        SnowEffect effect;

        SnowSprite[] snowSprites;

        public Snow(Effect effect, Texture2D texture, int snowCount)
        {
            if (effect == null) throw new ArgumentNullException("effect");
            if (texture == null) throw new ArgumentNullException("texture");

            this.effect = new SnowEffect(effect);
            this.effect.Texture = texture;

            snowSprites = new SnowSprite[snowCount];
            for (int i = 0; i < snowSprites.Length; i++)
                snowSprites[i] = new SnowSprite(-0.01f, 16, -5, 5, 0.01f, 0.05f);
        }

        public void Draw(ICamera camera)
        {
            foreach (var snowSprite in snowSprites) snowSprite.Move();

            var cameraPosition = camera.View.Position;
            Matrix world;
            Matrix.CreateTranslation(ref cameraPosition, out world);

            Matrix worldView;
            Matrix.Multiply(ref world, ref camera.View.Matrix, out worldView);

            foreach (var snowSprite in snowSprites) snowSprite.SetVertex(ref worldView);

            Array.Sort(snowSprites);

            effect.Projection = camera.Projection.Matrix;

            var graphicsDevice = effect.Effect.GraphicsDevice;
            graphicsDevice.BlendState = BlendState.AlphaBlend;

            foreach (var snowSprite in snowSprites) snowSprite.Draw(effect);
        }
    }
}
