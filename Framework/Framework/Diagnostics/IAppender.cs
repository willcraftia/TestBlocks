#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Diagnostics
{
    public interface IAppender
    {
        void Append(ref LogEvent logEvent);
    }
}
