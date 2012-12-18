#region Using

using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public struct MeshPartDefinition
    {
        [XmlArrayItem("Vertex")]
        public VertexPositionNormalTexture[] Vertices;

        [XmlArrayItem("Index")]
        public ushort[] Indices;
    }
}
