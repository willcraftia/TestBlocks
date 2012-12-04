#region Using

using System;
using System.IO;

#endregion

namespace Willcraftia.Xna.Framework.IO
{
    public interface IResourceContainer
    {
        bool ReadOnly { get; }

        bool Exists(Uri uri);

        Stream Open(Uri uri);

        Stream Create(Uri uri);

        void Delete(Uri uri);
    }
}
