#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class CubeMesh : PrimitiveMesh
    {
        static readonly Vector3[] normals =
        {
            Vector3.Up,
            Vector3.Down,
            Vector3.Forward,
            Vector3.Backward,
            Vector3.Left,
            Vector3.Right
        };

        public CubeMesh(GraphicsDevice graphicsDevice)
            : this(graphicsDevice, 1)
        {
        }

        public CubeMesh(GraphicsDevice graphicsDevice, float size)
        {
            var vertexCount = 6 * 4;
            var indexCount = 6 * 6;

            //================================================================
            // Allocate

            Allocate(vertexCount, indexCount);

            //================================================================
            // Vertex & Index

            foreach (Vector3 normal in normals)
            {
                var side1 = new Vector3(normal.Y, normal.Z, normal.X);
                var side2 = Vector3.Cross(normal, side1);

                // Index

                AddIndex(CurrentVertex + 0);
                AddIndex(CurrentVertex + 1);
                AddIndex(CurrentVertex + 2);

                AddIndex(CurrentVertex + 0);
                AddIndex(CurrentVertex + 2);
                AddIndex(CurrentVertex + 3);

                // Vertex

                AddVertex((normal - side1 - side2) * size / 2, normal);
                AddVertex((normal - side1 + side2) * size / 2, normal);
                AddVertex((normal + side1 + side2) * size / 2, normal);
                AddVertex((normal + side1 - side2) * size / 2, normal);
            }

            //================================================================
            // Build

            Build(graphicsDevice);
        }
    }
}
