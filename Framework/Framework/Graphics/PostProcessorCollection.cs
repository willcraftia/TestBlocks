#region Using

using System;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class PostProcessorCollection : ListBase<PostProcessor>
    {
        public PostProcessorCollection(int capacity) : base(capacity) { }
    }
}
