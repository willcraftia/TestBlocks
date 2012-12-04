#region Using

using System;
using System.IO;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.IO
{
    public sealed class TitleResourceContainer : IResourceContainer
    {
        public const string Scheme = "title";

        public static readonly TitleResourceContainer Instance = new TitleResourceContainer();

        public bool ReadOnly
        {
            get { return true; }
        }

        TitleResourceContainer() { }

        public bool Exists(Uri uri)
        {
            // 開発者の責任においてリソースの存在を保証する。
            return true;
        }

        public Stream Open(Uri uri)
        {
            return TitleContainer.OpenStream(uri.LocalPath);
        }

        public Stream Create(Uri uri)
        {
            throw new NotSupportedException();
        }

        public void Delete(Uri uri)
        {
            throw new NotSupportedException();
        }
    }
}
