#region Using

using System;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class ParticleSystemCollection : KeyedList<string, ParticleSystem>
    {
        public ParticleSystemCollection(int capacity)
            : base(capacity)
        {
        }

        protected override string GetKeyForItem(ParticleSystem item)
        {
            return item.Name;
        }
    }
}
