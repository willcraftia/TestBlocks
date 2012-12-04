#region Using

using System;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public sealed class MeshPartDefinition
    {
        public VertexPositionNormalTexture[] Vertices;

        public ushort[] Indices;
    }
}
