#if WINDOWS

#region Using

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using Willcraftia.Xna.Framework.Diagnostics;
using Willcraftia.Xna.Framework.IO;
using Willcraftia.Xna.Framework.Serialization;

#endregion

namespace Willcraftia.Xna.Framework.Content
{
    public sealed class WebAssetManager
    {
        public const int DefaultTimeout = 60000;

        static readonly Logger logger = new Logger(typeof(WebAssetManager).Name);

        static readonly string downloadListPath = "DownloadList.xml";

        static readonly XmlSerializerAdapter downloadListSerializer = new XmlSerializerAdapter(typeof(DownloadListDefinition));

        AssetManager assetManager;

        DownloadInfoCollection downloadInfoCollection = new DownloadInfoCollection();

        public int Timeout { get; set; }

        public WebAssetManager(AssetManager assetManager)
        {
            if (assetManager == null) throw new ArgumentNullException("assetManager");

            this.assetManager = assetManager;

            Timeout = DefaultTimeout;
        }

        public void LoadDownloadList()
        {
            var storageContainer = StorageManager.RequiredCurrentStorageContainer;

            if (!storageContainer.FileExists(downloadListPath)) return;

            using (var stream = storageContainer.OpenFile(downloadListPath, FileMode.Open))
            {
                var definition = (DownloadListDefinition) downloadListSerializer.Deserialize(stream, null);
                if (ArrayHelper.IsNullOrEmpty(definition.Entries)) return;

                for (int i = 0; i < definition.Entries.Length; i++)
                {
                    var downloadInfo = new DownloadInfo(definition.Entries[i].Uri)
                    {
                        LastModified = definition.Entries[i].LastModified
                    };
                    downloadInfoCollection.Add(downloadInfo);
                }
            }
        }

        public void SaveDownloadList()
        {
            var storageContainer = StorageManager.RequiredCurrentStorageContainer;

            using (var stream = storageContainer.CreateFile(downloadListPath))
            {
                var definition = new DownloadListDefinition();

                if (downloadInfoCollection.Count != 0)
                {
                    definition.Entries = new DownloadDefinition[downloadInfoCollection.Count];

                    for (int i = 0; i < downloadInfoCollection.Count; i++)
                    {
                        var downloadInfo = downloadInfoCollection[i];

                        definition.Entries[i] = new DownloadDefinition
                        {
                            Uri = downloadInfo.Uri,
                            LastModified = downloadInfo.LastModified
                        };
                    }
                }

                downloadListSerializer.Serialize(stream, definition);
            }
        }

        public object Load(WebResource resource, Type type)
        {
            ResolveLocalResource(resource);
            logger.Debug("Cache: {0}", resource.LocalResource.AbsoluteUri);

            // キャッシュが存在しない場合にのみ自動的にダウンロードを開始する。
            // タイムスタンプ比較からの自動的なダウンロード可否判定を行わない。
            // キャッシュが古い場合に最新をダウンロードするか否かは、ユーザ主導で決定する。
            // なお、ゲームでは、Region の初回選択時に発生するダウンロードのみとし、
            // ゲーム開始後に最新をダウンロードすることは想定しない。
            // エディタでは、最新を確認した後、ユーザ判断により再ダウンロードが発生しうる。
            if (!resource.LocalResource.Exists())
            {
                logger.Debug("No cache exists.");

                Download(resource);
            }
            else
            {
                logger.Debug("Cache exists.");
            }

            return assetManager.Load(resource.LocalResource, type);
        }

        public long GetLastModified(WebResource resource)
        {
            var request = WebRequest.Create(resource.AbsoluteUri);
            request.Timeout = Timeout;

            using (var response = request.GetResponse() as HttpWebResponse)
            {
                return response.LastModified.Ticks;
            }
        }

        public void Download(WebResource resource)
        {
            logger.Info("Download: {0}", resource.AbsoluteUri);

            var request = WebRequest.Create(resource.AbsoluteUri);
            request.Timeout = Timeout;

            using (var response = request.GetResponse() as HttpWebResponse)
            {
                // just debug.
                LogHttpResponseHeaders(response);

                using (var webStream = response.GetResponseStream())
                using (var localStream = resource.LocalResource.Create())
                {
                    int value;
                    while (-1 < (value = webStream.ReadByte()))
                    {
                        localStream.WriteByte((byte) value);
                    }
                }

                // TODO: what kind of the time is set?
                var downloadInfo = GetDownloadInfo(resource);
                downloadInfo.LastModified = response.LastModified.Ticks;
            }
        }

        DownloadInfo GetDownloadInfo(WebResource resource)
        {
            if (downloadInfoCollection.Contains(resource.AbsoluteUri))
            {
                return downloadInfoCollection[resource.AbsoluteUri];
            }
            else
            {
                var downloadInfo = new DownloadInfo(resource.AbsoluteUri);
                downloadInfoCollection.Add(downloadInfo);
                return downloadInfo;
            }
        }

        [Conditional("DEBUG")]
        void LogHttpResponseHeaders(WebResponse response)
        {
            foreach (var key in response.Headers.AllKeys)
            {
                var value = response.Headers[key];
                logger.Debug("[{0}] {1}", key, value);
            }
        }

        void ResolveLocalResource(WebResource resource)
        {
            if (resource.LocalResource != null) return;

            var localUri = CreateLocalUri(resource.AbsoluteUri);
            resource.LocalResource = StorageResourceLoader.Instance.LoadResource(localUri) as StorageResource;
        }

        string CreateLocalUri(string uri)
        {
            var b = new StringBuilder();
            b.Append(StorageResource.StorageScheme);
            b.Append(":Downloads/");
            for (int i = 0; i < uri.Length; i++)
            {
                var c = uri[i];

                switch (c)
                {
                    case ':':
                    case '/':
                    case '!':
                    case '#':
                    case '?':
                        b.Append('_');
                        break;
                    default:
                        b.Append(c);
                        break;
                }
            }
            return b.ToString();
        }
    }
}

#endif