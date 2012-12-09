#region Using

using System;
using System.IO;

#endregion

namespace Willcraftia.Xna.Framework
{
    public interface IUri
    {
        string AbsoluteUri { get; }

        string Scheme { get; }

        string AbsolutePath { get; }

        string Extension { get; }

        bool ReadOnly { get; }

        string BaseUri { get; }

        Stream Open();

        Stream Create();

        void Delete();
    }
}
