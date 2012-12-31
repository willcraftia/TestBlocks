#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public abstract class SceneObject
    {
        public Vector3 Position;

        public BoundingBox BoundingBox;

        public BoundingSphere BoundingSphere;

        public ISceneObjectContext Context { get; set; }

        public bool Visible { get; set; }

        public bool Translucent { get; set; }

        public bool Occluded { get; protected set; }

        protected SceneObject()
        {
            Visible = true;
        }

        public virtual void UpdateOcclusion() { }

        public abstract void Draw();
    }
}
