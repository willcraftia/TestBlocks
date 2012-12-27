﻿#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class SphereMesh : PrimitiveMesh
    {
        public SphereMesh(GraphicsDevice graphicsDevice)
            : this(graphicsDevice, 1, 16)
        {
        }

        public SphereMesh(GraphicsDevice graphicsDevice, float diameter, int tessellation)
        {
            if (tessellation < 3) throw new ArgumentOutOfRangeException("tessellation");

            var verticalSegments = tessellation;
            var horizontalSegments = tessellation * 2;
            var radius = diameter / 2;

            var vertexCount = (verticalSegments - 1) * horizontalSegments + 2;
            var indexCount = horizontalSegments * 6 + (verticalSegments - 2) * horizontalSegments * 6;

            //================================================================
            // Allocate

            Allocate(vertexCount, indexCount);

            //================================================================
            // Vertex

            AddVertex(Vector3.Down * radius, Vector3.Down);

            for (int i = 0; i < verticalSegments - 1; i++)
            {
                var latitude = ((i + 1) * MathHelper.Pi / verticalSegments) - MathHelper.PiOver2;
                var dy = (float) Math.Sin(latitude);
                var dxz = (float) Math.Cos(latitude);

                for (int j = 0; j < horizontalSegments; j++)
                {
                    var longitude = j * MathHelper.TwoPi / horizontalSegments;
                    var dx = (float) Math.Cos(longitude) * dxz;
                    var dz = (float) Math.Sin(longitude) * dxz;
                    var normal = new Vector3(dx, dy, dz);

                    AddVertex(normal * radius, normal);
                }
            }

            AddVertex(Vector3.Up * radius, Vector3.Up);

            //================================================================
            // Index

            for (int i = 0; i < horizontalSegments; i++)
            {
                AddIndex(0);
                AddIndex(1 + (i + 1) % horizontalSegments);
                AddIndex(1 + i);
            }

            for (int i = 0; i < verticalSegments - 2; i++)
            {
                for (int j = 0; j < horizontalSegments; j++)
                {
                    var nextI = i + 1;
                    var nextJ = (j + 1) % horizontalSegments;

                    AddIndex(1 + i * horizontalSegments + j);
                    AddIndex(1 + i * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + j);

                    AddIndex(1 + i * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + j);
                }
            }

            for (int i = 0; i < horizontalSegments; i++)
            {
                AddIndex(CurrentVertex - 1);
                AddIndex(CurrentVertex - 2 - (i + 1) % horizontalSegments);
                AddIndex(CurrentVertex - 2 - i);
            }

            //================================================================
            // Build

            Build(graphicsDevice);
        }
    }
}
