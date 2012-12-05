#region Using

using System;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public struct MeshPartDefinition
    {
        public VertexPositionNormalTexture[] Vertices;

        public ushort[] Indices;
    }
}
