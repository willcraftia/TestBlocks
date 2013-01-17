#region Using

using System;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class SceneObjectCollection : ListBase<SceneObject>
    {
        ISceneObjectContext context;

        public SceneObjectCollection(ISceneObjectContext context, int capacity)
            : base(capacity)
        {
            if (context == null) throw new ArgumentNullException("context");

            this.context = context;
        }

        protected override void InsertOverride(int index, SceneObject item)
        {
            item.Context = context;

            base.InsertOverride(index, item);
        }

        protected override void SetOverride(int index, SceneObject item)
        {
            this[index].Context = null;
            item.Context = context;

            base.SetOverride(index, item);
        }

        protected override void RemoveAtOverride(int index)
        {
            this[index].Context = null;

            base.RemoveAtOverride(index);
        }
    }
}
