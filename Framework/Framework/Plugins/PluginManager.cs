#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Plugins
{
    public static class PluginManager
    {
        //
        // Initialize() によるプラグインの一括ロードのみに対応。
        // ロードされたプラグインは、アプリケーション終了まで存在。
        // 管理と実装が非常に複雑になるため、アンロードを考慮しない。
        // なお、実現するにしても、.NET Compact Framework for XNA では不可能な可能性がある。
        //

        static readonly Logger logger = new Logger(typeof(PluginManager).Name);

        static readonly Type pluginInterfaceType = typeof(IPlugin);

        static bool initialized;

        // プラグイン情報のデバッグのためにリストで保持。
        // プラグインの型からアセンブリ情報などを得られる。
        static List<IPlugin> plugins = new List<IPlugin>();

        public static PluginHostRegistory HostRegistory { get; private set; }

        static PluginManager()
        {
            HostRegistory = new PluginHostRegistory();
        }

        public static IEnumerable<IPlugin> EnumeratePlugins()
        {
            return plugins;
        }

        public static void Initialize()
        {
            if (initialized) throw new InvalidOperationException("PluginManager is already initialized.");

            logger.Info("Initialize");

            var pluginDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            if (!Directory.Exists(pluginDirectoryPath))
            {
                logger.Warn("Directory not found: {0}", pluginDirectoryPath);
                initialized = true;
                return;
            }

            var dlls = Directory.GetFiles(pluginDirectoryPath, "*.dll");
            foreach (var dll in dlls)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dll);

                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsClass && type.IsPublic && !type.IsAbstract &&
                            pluginInterfaceType.IsAssignableFrom(type))
                        {
                            var typeName = type.FullName;

                            try
                            {
                                var plugin = assembly.CreateInstance(typeName) as IPlugin;
                                plugin.Load(HostRegistory);

                                plugins.Add(plugin);

                                logger.Info("Plugin Loaded: {0}", typeName);
                            }
                            catch (Exception e)
                            {
                                logger.Error(e, "Error loading plugin: {0}", typeName);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error loading assembly: {0}", dll);
                }
            }

            initialized = true;
        }
    }
}
