#region Using

using System;
using System.IO;

#endregion

namespace Willcraftia.Xna.Framework.IO
{
    public sealed class FileResourceContainer : IResourceContainer
    {
        public const string Scheme = "file";

        public static readonly FileResourceContainer Instance = new FileResourceContainer();

        public bool ReadOnly
        {
            get { return false; }
        }

        FileResourceContainer() { }

        public bool Exists(Uri uri)
        {
            return File.Exists(uri.LocalPath);
        }

        public Stream Open(Uri uri)
        {
            return File.Open(uri.LocalPath, FileMode.Open);
        }

        public Stream Create(Uri uri)
        {
            return File.Create(uri.LocalPath);
        }

        public void Delete(Uri uri)
        {
            File.Delete(uri.LocalPath);
        }
    }
}
