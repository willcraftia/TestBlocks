#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class LensFlare : IDisposable
    {
        #region Flare

        class Flare
        {
            public float Position;
            
            public float Scale;
            
            public Color Color;

            public int TextureIndex;

            public Texture2D Texture;

            public Flare(float position, float scale, Color color, int textureIndex)
            {
                Position = position;
                Scale = scale;
                Color = color;
                TextureIndex = textureIndex;
            }
        }

        #endregion

        const float querySize = 100;

        const float glowSize = 400;

        static readonly BlendState ColorWriteDisable = new BlendState
        {
            ColorWriteChannels = ColorWriteChannels.None
        };

        SpriteBatch spriteBatch;

        Texture2D glowSprite;

        Flare[] flares =
        {
            new Flare(-0.5f, 0.7f, new Color( 50,  25,  50), 0),
            new Flare( 0.3f, 0.4f, new Color(100, 255, 200), 0),
            new Flare( 1.2f, 1.0f, new Color(100,  50,  50), 0),
            new Flare( 1.5f, 1.5f, new Color( 50, 100,  50), 0),

            new Flare(-0.3f, 0.7f, new Color(200,  50,  50), 1),
            new Flare( 0.6f, 0.9f, new Color( 50, 100,  50), 1),
            new Flare( 0.7f, 0.4f, new Color( 50, 200, 200), 1),

            new Flare(-0.7f, 0.7f, new Color( 50, 100,  25), 2),
            new Flare( 0.0f, 0.6f, new Color( 25,  25,  25), 2),
            new Flare( 2.0f, 1.4f, new Color( 25,  50, 100), 2),
        };

        BasicEffect basicEffect;

        VertexPositionColor[] queryVertices;

        OcclusionQuery occlusionQuery;

        bool occlusionQueryActive;
        
        float occlusionAlpha;

        Vector3 lightDirection;

        Vector2 lightPosition;

        bool lightBehindCamera;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public LensFlare(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Texture2D glowSprite, Texture2D[] flareSprites)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (spriteBatch == null) throw new ArgumentNullException("spriteBatch");
            if (glowSprite == null) throw new ArgumentNullException("glowSprite");
            if (flareSprites == null) throw new ArgumentNullException("flareSprites");

            GraphicsDevice = graphicsDevice;
            this.spriteBatch = spriteBatch;
            this.glowSprite = glowSprite;

            for (int i = 0; i < flares.Length; i++)
            {
                var index = flares[i].TextureIndex;
                if (index < 0 || flareSprites.Length <= index)
                    throw new InvalidOperationException("Invalid index of flare sprite: " + index);

                flares[i].Texture = flareSprites[index];
            }

            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.View = Matrix.Identity;
            basicEffect.VertexColorEnabled = true;

            queryVertices = new VertexPositionColor[4];
            queryVertices[0].Position = new Vector3(-querySize / 2, -querySize / 2, -1);
            queryVertices[1].Position = new Vector3( querySize / 2, -querySize / 2, -1);
            queryVertices[2].Position = new Vector3(-querySize / 2,  querySize / 2, -1);
            queryVertices[3].Position = new Vector3( querySize / 2,  querySize / 2, -1);

            occlusionQuery = new OcclusionQuery(GraphicsDevice);
        }

        public void Draw(ICamera viewerCamera, Vector3 lightDirection)
        {
            this.lightDirection = lightDirection;

            var infiniteView = viewerCamera.View.Matrix;
            infiniteView.Translation = Vector3.Zero;

            var viewport = GraphicsDevice.Viewport;
            var projectedPosition = viewport.Project(-lightDirection, viewerCamera.Projection.Matrix, infiniteView, Matrix.Identity);

            if (projectedPosition.Z < 0 || 1 < projectedPosition.Z)
            {
                lightBehindCamera = true;
                return;
            }

            lightPosition = new Vector2(projectedPosition.X, projectedPosition.Y);
            lightBehindCamera = false;

            UpdateOcclusion();

            DrawGlow();
            DrawFlares();

            RestoreRenderStates();
        }

        public void UpdateOcclusion()
        {
            if (lightBehindCamera) return;

            if (occlusionQueryActive)
            {
                if (!occlusionQuery.IsComplete) return;

                const float queryArea = querySize * querySize;
                occlusionAlpha = Math.Min(occlusionQuery.PixelCount / queryArea, 1);
            }

            var viewport = GraphicsDevice.Viewport;

            basicEffect.World = Matrix.CreateTranslation(lightPosition.X, lightPosition.Y, 0);
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            basicEffect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.BlendState = ColorWriteDisable;
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            occlusionQuery.Begin();

            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, queryVertices, 0, 2);

            occlusionQuery.End();

            occlusionQueryActive = true;
        }

        public void DrawGlow()
        {
            if (lightBehindCamera || occlusionAlpha <= 0) return;

            var color = Color.White * occlusionAlpha;
            var origin = new Vector2(glowSprite.Width, glowSprite.Height) / 2;
            var scale = glowSize * 2 / glowSprite.Width;

            spriteBatch.Begin();
            spriteBatch.Draw(glowSprite, lightPosition, null, color, 0, origin, scale, SpriteEffects.None, 0);
            spriteBatch.End();
        }

        public void DrawFlares()
        {
            if (lightBehindCamera || occlusionAlpha <= 0) return;

            var viewport = GraphicsDevice.Viewport;

            var screenCenter = new Vector2(viewport.Width, viewport.Height) / 2;

            var flareVector = screenCenter - lightPosition;

            spriteBatch.Begin(0, BlendState.Additive);

            foreach (var flare in flares)
            {
                var flarePosition = lightPosition + flareVector * flare.Position;

                var flareColor = flare.Color.ToVector4();
                flareColor.W *= occlusionAlpha;

                var flareOrigin = new Vector2(flare.Texture.Width, flare.Texture.Height) / 2;

                spriteBatch.Draw(flare.Texture, flarePosition, null, new Color(flareColor),
                    1, flareOrigin, flare.Scale, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }

        void RestoreRenderStates()
        {
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~LensFlare()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                basicEffect.Dispose();
                occlusionQuery.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
