#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class Quad
    {
        VertexPositionTexture[] vertices =
        {
            new VertexPositionTexture(new Vector3(0,0,0), new Vector2(1,1)),
            new VertexPositionTexture(new Vector3(0,0,0), new Vector2(0,1)),
            new VertexPositionTexture(new Vector3(0,0,0), new Vector2(0,0)),
            new VertexPositionTexture(new Vector3(0,0,0), new Vector2(1,0))
        };

        short[] indices = { 0, 1, 2, 2, 3, 0 };

        public GraphicsDevice GraphicsDevice { get; private set; }

        public Quad(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            GraphicsDevice = graphicsDevice;
        }

        public void Draw(Vector2 v1, Vector2 v2)
        {
            vertices[0].Position.X = v2.X;
            vertices[0].Position.Y = v1.Y;

            vertices[1].Position.X = v1.X;
            vertices[1].Position.Y = v1.Y;

            vertices[2].Position.X = v1.X;
            vertices[2].Position.Y = v2.Y;

            vertices[3].Position.X = v2.X;
            vertices[3].Position.Y = v2.Y;

            GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(
                PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2);
        }
    }
}
