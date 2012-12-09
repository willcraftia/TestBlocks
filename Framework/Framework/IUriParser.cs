#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework
{
    public interface IUriParser
    {
        bool CanParse(string uri);

        IUri Parse(string uri);
    }
}
