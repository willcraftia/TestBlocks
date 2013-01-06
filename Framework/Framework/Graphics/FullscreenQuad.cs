#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class FullscreenQuad
    {
        static readonly VertexPositionTexture[] vertices =
        {
            new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0))
        };

        static readonly short[] indices = { 0, 1, 2, 2, 3, 0 };

        VertexBuffer vertexBuffer;

        IndexBuffer indexBuffer;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public FullscreenQuad(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            GraphicsDevice = graphicsDevice;

            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        public void Draw()
        {
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
        }
    }
}
