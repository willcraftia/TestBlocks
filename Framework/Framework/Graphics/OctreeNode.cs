#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class OctreeNode : SceneNode
    {
        public Octree Octree { get; internal set; }

        public OctreeNode(SceneManager manager, string name)
            : base(manager, name)
        {
        }
    }
}
