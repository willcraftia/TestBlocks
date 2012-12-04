#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Assets
{
    public sealed class AssetManager : IDisposable
    {
        public const string ContentScheme = "content";

        static readonly Logger logger = new Logger(typeof(AssetManager).Name);

        NoCacheContentManager contentManager;

        Dictionary<string, Uri> stringUriMap = new Dictionary<string, Uri>();

        AssetHolderCollection holders = new AssetHolderCollection();

        public Dictionary<Type, IAssetLoader> LoaderMap { get; private set; }

        public string ContentRootDirectory
        {
            get { return contentManager.RootDirectory; }
            set { contentManager.RootDirectory = value; }
        }

        public AssetManager(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");

            contentManager = new NoCacheContentManager(serviceProvider);
            LoaderMap = new Dictionary<Type, IAssetLoader>();
        }

        public Uri CreateUri(string uriString)
        {
            // Try to use a cached Uri instance.
            Uri uri;
            if (!stringUriMap.TryGetValue(uriString, out uri))
                uri = new Uri(uriString);

            return uri;
        }

        public T Load<T>(string uriString)
        {
            return Load<T>(CreateUri(uriString));
        }

        public T Load<T>(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            // Use the cached asset or create a new one.
            AssetHolder holder;
            if (!holders.TryGetItem(uri, out holder)) holder = LoadNew<T>(uri);

            return (T) holder.Asset;
        }

        AssetHolder LoadNew<T>(Uri uri)
        {
            logger.InfoBegin("LoadNew: {0}", uri);

            object asset;
            IAssetLoader loader;
            if (ContentScheme == uri.Scheme)
            {
                // XNA content framework
                loader = null;
                asset = contentManager.Load<T>(uri.LocalPath);
            }
            else
            {
                // My asset framework
                loader = GetLoader(typeof(T));
                asset = loader.Load(this, uri);
            }

            // Cache
            var holder = new AssetHolder { Uri = uri, Asset = asset, Loader = loader };
            holders.Add(holder);
            stringUriMap[uri.OriginalString] = uri;

            logger.InfoEnd("LoadNew: {0}", uri);

            return holder;
        }

        public void Unload(string uriString)
        {
            Unload(CreateUri(uriString));
        }

        public void Unload(Uri uri)
        {
            logger.InfoBegin("Unload: {0}", uri);

            AssetHolder holder;
            if (holders.TryGetItem(uri, out holder))
            {
                if (holder.Loader == null)
                {
                    DisposeIfNeeded(holder);
                }
                else
                {
                    holder.Loader.Unload(this, holder.Uri, holder.Asset);
                }

                holders.Remove(uri);
                stringUriMap.Remove(uri.OriginalString);

                holder.Uri = null;
                holder.Asset = null;
                holder.Loader = null;
            }

            logger.InfoEnd("Unload: {0}", uri);
        }

        public void Unload()
        {
            if (holders.Count == 0) return;

            while (0 < holders.Count)
                Unload(holders[holders.Count - 1].Uri);
        }

        // ※保存に関する注意
        //
        // アセットはキャッシュされるため、一度ロードしたアセットを編集し、これを異なる URI で保存しようとすると、
        // 二つの URI にアセットが関連付けられることとなる。
        //
        // このような関連付けの問題を起こさないために、AssetManager は、アセットが保存要求とは異なる URI に関連付いている場合、
        // その関連付けを破棄してから保存を行う。
        // これは、キャッシュの破棄であり、アセットに対する Dipose 処理は伴わない。
        // 単なるキャッシュの破棄であれば、過去の URI でアセットを得たい場合、Load 処理により新たなアセットがインスタンス化される。
        //
        // 別の問題として、保存の際に異なるアセットが既に URI に関連づいている場合、
        // URI をキーとして間接的に参照しているコードとの不整合が発生する。
        //
        // この解決のために、URI に関連付くアセットがあり、これが保存しようとするアセットとは異なる場合、
        // AssetManager は保存を拒否する。
        // 通常、アセットは他のクラスでも参照しているため、既存のアセットを AssetManager の判断では処理できない。
        // このため、エディタなどの保存要求を出すクラスでは、
        // 上書きをしたい場合には事前に対象となるアセットの削除処理を行わせる必要がある。
        // この削除処理では、単に AssetManager でアセットを削除するだけでなく、
        // 削除したいアセットを参照しているクラスから、その参照を取り除く必要がある。

        public void Save<T>(string uriString, T asset)
        {
            Save<T>(CreateUri(uriString), asset);
        }

        public void Save<T>(Uri uri, T asset)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (asset == null) throw new ArgumentNullException("asset");
            if (ContentScheme == uri.Scheme)
                throw new ArgumentException(string.Format("The scheme '{0}' locates a read-only asset.", uri.Scheme));

            logger.InfoBegin("Save: {0}", uri);

            // Protect.
            AssetHolder testHolder;
            if (holders.TryGetItem(uri, out testHolder) && !asset.Equals(testHolder.Asset))
                throw new InvalidOperationException(
                    string.Format("The specified uri '{0}' is bound to the other asset '{0}'.", uri));

            // Unbind if needed.
            AssetHolder oldHolder = null;
            foreach (var assetHolder in holders)
            {
                if (assetHolder.Asset.Equals(asset) && assetHolder.Uri != uri)
                {
                    oldHolder = assetHolder;
                    break;
                }
            }

            if (oldHolder != null)
            {
                holders.Remove(oldHolder);

                logger.Info("Old asset holder removed: {0}", oldHolder.Uri);
            }

            // Save.
            var loader = GetLoader(typeof(T));
            loader.Save(this, uri, asset);

            // Prepare the asset holder.
            var newHolder = oldHolder ?? new AssetHolder();
            newHolder.Uri = uri;
            newHolder.Asset = asset;
            newHolder.Loader = loader;

            // Cache.
            holders.Add(newHolder);
            stringUriMap[uri.OriginalString] = uri;

            logger.InfoEnd("Save: {0}", uri);
        }

        IAssetLoader GetLoader(Type type)
        {
            IAssetLoader result;
            if (TryGetLoader(type, out result))
                return result;

            throw new InvalidOperationException(string.Format("The asset loader for '{0}' can not be found.", type));
        }

        bool TryGetLoader(Type type, out IAssetLoader result)
        {
            if (LoaderMap.TryGetValue(type, out result))
                return true;

            // By the interface.
            foreach (var interfaceType in type.GetInterfaces())
            {
                if (TryGetLoader(interfaceType, out result))
                    return true;
            }

            // By the base type.
            if (type.BaseType != null && TryGetLoader(type.BaseType, out result))
                return true;

            return false;
        }

        void DisposeIfNeeded(AssetHolder holder)
        {
            var disposable = holder.Asset as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~AssetManager()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            Unload();
            contentManager.Dispose();

            disposed = true;
        }

        #endregion
    }
}
