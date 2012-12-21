#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Diagnostics
{
    public sealed class BoundingBoxDrawer
    {
        static readonly int[] indices;

        static readonly int primitiveCount;

        static BoundingBoxDrawer()
        {
            indices = new int[]
            {
                0, 1,
                1, 2,
                2, 3,
                3, 0,
                0, 4,
                1, 5,
                2, 6,
                3, 7,
                4, 5,
                5, 6,
                6, 7,
                7, 4,
            };
            primitiveCount = indices.Length / 2;
        }

        GraphicsDevice graphicsDevice;
        
        Vector3[] corners = new Vector3[8];
        
        VertexPositionColor[] vertices = new VertexPositionColor[8];

        Color color = Color.White;

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public BoundingBoxDrawer(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            this.graphicsDevice = graphicsDevice;
        }

        public void Draw(ref BoundingBox box, Effect effect)
        {
            Draw(ref box, effect, ref color);
        }

        public void Draw(ref BoundingBox box, Effect effect, ref Color vertexColor)
        {
            if (effect == null) throw new ArgumentNullException("effect");

            box.GetCorners(corners);
            for (int i = 0; i < 8; i++)
            {
                vertices[i].Position = corners[i];
                vertices[i].Color = vertexColor;
            }

            effect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                PrimitiveType.LineList,
                vertices,
                0,
                8,
                indices,
                0,
                primitiveCount);
        }
    }
}
