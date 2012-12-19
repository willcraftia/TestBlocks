#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Framework.Content
{
    public sealed class AssetManager : IDisposable
    {
        static readonly Logger logger = new Logger(typeof(AssetManager).Name);

        NoCacheContentManager contentManager;

#if WINDOWS
        WebAssetManager webAssetManager;
#endif

        AssetHolderCollection holders = new AssetHolderCollection();

        Dictionary<Type, IAssetLoader> loaderMap;

        public string ContentRootDirectory
        {
            get { return contentManager.RootDirectory; }
            set { contentManager.RootDirectory = value; }
        }

        public AssetManager(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");

            contentManager = new NoCacheContentManager(serviceProvider);
            contentManager.RootDirectory = "Content";
            loaderMap = new Dictionary<Type, IAssetLoader>();

#if WINDOWS
            webAssetManager = new WebAssetManager(this);
#endif
        }

        public void RegisterLoader(Type type, IAssetLoader loader)
        {
            loaderMap[type] = loader;

            var assetManagerAware = loader as IAssetManagerAware;
            if (assetManagerAware != null) assetManagerAware.AssetManager = this;
        }

        public bool Unregister(Type type)
        {
            IAssetLoader loader;
            if (loaderMap.TryGetValue(type, out loader))
            {
                var assetManagerAware = loader as IAssetManagerAware;
                if (assetManagerAware != null) assetManagerAware.AssetManager = null;
                
                loaderMap.Remove(type);
                return true;
            }

            return false;
        }

        public T Load<T>(IResource resource)
        {
            return (T) Load(resource, typeof(T));
        }

        public object Load(IResource resource, Type type)
        {
            if (resource == null) throw new ArgumentNullException("resource");

            AssetHolder holder;
            if (!holders.TryGetItem(resource, out holder)) holder = LoadNew(resource, type);

            return holder.Asset;
        }

        AssetHolder LoadNew(IResource resource, Type type)
        {
            logger.Info("LoadNew: {0}", resource);

            object asset;
            if (resource is ContentResource)
            {
                // XNA content framework
                asset = contentManager.Load<object>(resource.AbsolutePath);
            }
            else if (resource is WebResource)
            {
                // Web resource.
#if WINDOWS
                asset = webAssetManager.Load(resource as WebResource, type);
#else
                throw new NotSupportedException();
#endif
            }
            else
            {
                // My asset framework
                asset = GetLoader(type).Load(resource);
            }

            var assetInterface = asset as IAsset;
            if (assetInterface != null) assetInterface.Resource = resource;

            // Cache
            var holder = new AssetHolder { Resource = resource, Asset = asset };
            holders.Add(holder);

            return holder;
        }

        public void Unload(IResource resource)
        {
            logger.Info("Unload: {0}", resource);

            AssetHolder holder;
            if (holders.TryGetItem(resource, out holder))
            {
                DisposeIfNeeded(holder);
                holders.Remove(resource);
                holder.Resource = null;
                holder.Asset = null;
            }
        }

        public void Unload()
        {
            if (holders.Count == 0) return;

            while (0 < holders.Count)
                Unload(holders[holders.Count - 1].Resource);
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

        public void Save<T>(IResource resource, T asset)
        {
            if (resource == null) throw new ArgumentNullException("resource");
            if (asset == null) throw new ArgumentNullException("asset");
            if (resource.ReadOnly) throw new InvalidOperationException("Read-only resource: " + resource);

            logger.Info("Save: {0}", resource);

            // Protect.
            AssetHolder testHolder;
            if (holders.TryGetItem(resource, out testHolder) && !asset.Equals(testHolder.Asset))
                throw new InvalidOperationException("Resource '{0}' is bound to the other asset: " + resource);

            // Unbind if needed.
            AssetHolder oldHolder = null;
            foreach (var assetHolder in holders)
            {
                if (assetHolder.Asset.Equals(asset) && assetHolder.Resource != resource)
                {
                    oldHolder = assetHolder;
                    break;
                }
            }

            if (oldHolder != null)
            {
                holders.Remove(oldHolder);

                logger.Info("Old asset holder removed: {0}", oldHolder.Resource);
            }

            // Save.
            var loader = GetLoader(typeof(T));
            loader.Save(resource, asset);

            // Prepare the asset holder.
            var newHolder = oldHolder ?? new AssetHolder();
            newHolder.Resource = resource;
            newHolder.Asset = asset;

            // Cache.
            holders.Add(newHolder);

            var assetInterface = asset as IAsset;
            if (assetInterface != null) assetInterface.Resource = resource;
        }

        IAssetLoader GetLoader(Type type)
        {
            IAssetLoader result;
            if (TryGetLoader(type, out result))
                return result;

            throw new InvalidOperationException("Asset loader not found: " + type);
        }

        bool TryGetLoader(Type type, out IAssetLoader result)
        {
            if (loaderMap.TryGetValue(type, out result))
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
