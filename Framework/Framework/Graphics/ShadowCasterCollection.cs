#region Using

using System;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class ShadowCasterCollection : ListBase<ShadowCaster>
    {
        ISceneObjectContext context;

        public ShadowCasterCollection(ISceneObjectContext context, int capacity)
            : base(capacity)
        {
        }

        protected override void InsertOverride(int index, ShadowCaster item)
        {
            item.Context = context;

            base.InsertOverride(index, item);
        }

        protected override void SetOverride(int index, ShadowCaster item)
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
