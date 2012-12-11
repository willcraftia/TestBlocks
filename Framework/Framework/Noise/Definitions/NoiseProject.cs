#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

#endregion

namespace Willcraftia.Xna.Framework.Noise.Definitions
{
    public sealed class NoiseProject
    {
        #region Holder

        struct Holder
        {
            public string Name;

            public INoiseSource NoiseSource;

            public NoiseSourceInfo NoiseSourceInfo;
        }

        #endregion

        #region HolderCollection

        class HolderCollection : KeyedCollection<string, Holder>
        {
            protected override string GetKeyForItem(Holder item)
            {
                return item.Name;
            }
        }

        #endregion

        NoiseSourceInfoCollection noiseSourceInfos = new NoiseSourceInfoCollection();

        HolderCollection holders = new HolderCollection();

        public string RootName { get; set; }

        public INoiseSource RootSource
        {
            get { return this[RootName]; }
        }

        public INoiseSource this[string name]
        {
            get
            {
                if (name == null) throw new ArgumentNullException("name");
                return holders[name].NoiseSource;
            }
        }

        //
        // エディタでは直接 INoiseSource をインスタンス化せずに、
        // NoiseProject に生成とバインディングを委任する。
        //
        // NoiseProject は、INoiseSource を名前付きで管理する。
        // 名前の重複は許可せず、同一の名前に対する INoiseSource の上書き設定も許可しない。
        // エディタでも同様に、名前の重複や上書き設定を拒否したいはずである。
        // 上書き設定が必要な場合、削除を行なってから追加を行う。
        //

        public NoiseSourceInfo CreateNoiseSourceInfo(string alias)
        {
            var type = NoiseSourceTypeAlias.GetType(alias);
            if (type == null) throw new ArgumentException("Unknown alias: " + alias);

            return CreateNoiseSourceInfo(type);
        }

        public NoiseSourceInfo CreateNoiseSourceInfo(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            if (noiseSourceInfos.Contains(type)) return noiseSourceInfos[type];

            var noiseSourceInfo = new NoiseSourceInfo(type);
            noiseSourceInfos.Add(noiseSourceInfo);
            return noiseSourceInfo;
        }

        public void Add(string name, NoiseSourceInfo noiseSourceInfo)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (noiseSourceInfo == null) throw new ArgumentNullException("noiseSourceInfo");

            var noiseSource = noiseSourceInfo.CreateInstance();
            
            var holder = new Holder
            {
                Name = name,
                NoiseSource = noiseSource,
                NoiseSourceInfo = noiseSourceInfo
            };

            holders.Add(holder);
        }

        public void SetParameter(string targetName, string propertyName, object propertyValue)
        {
            if (targetName == null) throw new ArgumentNullException("targetName");
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            if (!holders.Contains(targetName)) throw new ArgumentException("Source not found: " + targetName);

            var holder = holders[targetName];
            var noiseSourceInfo = holder.NoiseSourceInfo;
            var noiseSource = holder.NoiseSource;

            noiseSourceInfo.SetParameter(holder.NoiseSource, propertyName, propertyValue);
        }

        public void SetReference(string targetName, string propertyName, string referenceName)
        {
            if (targetName == null) throw new ArgumentNullException("targetName");
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            if (!holders.Contains(targetName)) throw new ArgumentException("Target not found: " + targetName);
            if (!holders.Contains(referenceName)) throw new ArgumentException("Reference not found: " + referenceName);

            var target = holders[targetName];
            var reference = holders[referenceName];

            target.NoiseSourceInfo.SetReference(target.NoiseSource, propertyName, reference.NoiseSource);
        }

        public void UnbindReference(string targetName, string propertyName)
        {
            if (targetName == null) throw new ArgumentNullException("targetName");
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            if (!holders.Contains(targetName)) throw new ArgumentException("Target not found: " + targetName);

            var target = holders[targetName];

            target.NoiseSourceInfo.SetReference(target.NoiseSource, propertyName, null);
        }

        public void Remove(string name)
        {
            if (!holders.Contains(name)) return;

            var removingNoiseSource = holders[name].NoiseSource;

            for (int i = 0; i < holders.Count; i++)
            {
                var cursorHolder = holders[i];
                if (cursorHolder.Name == name) continue;

                // Remove all references.
                var cursorNoiseSource = cursorHolder.NoiseSource;
                var cursorNoiseSourceInfo = cursorHolder.NoiseSourceInfo;
                cursorNoiseSourceInfo.UnbindReference(cursorNoiseSource, removingNoiseSource);
            }

            holders.Remove(name);
        }

        //
        // NoiseProject で管理している INoiseSource を破棄するのみ。
        // NoiseProject で管理している INoiseSource への参照をエディタが持っていた場合、
        // Clear() での破棄と同時にエディタが持つ参照を破棄することは、エディタの責務とする。

        public void Clear()
        {
            holders.Clear();
        }

        public void SetDefinition(ref NoiseProjectDefinition projectDefinition)
        {
            RootName = projectDefinition.RootName;

            if (ArrayHelper.IsNullOrEmpty(projectDefinition.Sources)) return;

            // Register INoiseSource instance.
            for (int i = 0; i < projectDefinition.Sources.Length; i++)
            {
                // Alias.
                var sourceType = NoiseSourceTypeAlias.GetType(projectDefinition.Sources[i].Type);

                // Full name.
                if (sourceType == null)
                    sourceType = Type.GetType(projectDefinition.Sources[i].Type);

                if (sourceType == null) throw new InvalidOperationException("Type not found: " + sourceType);

                var noiseSourceInfo = CreateNoiseSourceInfo(sourceType);
                var sourceName = projectDefinition.Sources[i].Name;

                // Add INoiseSource instance.
                Add(sourceName, noiseSourceInfo);

                // Populate INoiseSource instance with parameters.
                if (!ArrayHelper.IsNullOrEmpty(projectDefinition.Sources[i].Parameters))
                {
                    for (int j = 0; j < projectDefinition.Sources[i].Parameters.Length; j++)
                    {
                        var parameterDefinition = projectDefinition.Sources[i].Parameters[j];
                        SetParameter(sourceName, parameterDefinition.Name, parameterDefinition.Value);
                    }
                }
            }

            // Bind references.
            for (int i = 0; i < projectDefinition.Sources.Length; i++)
            {
                if (!ArrayHelper.IsNullOrEmpty(projectDefinition.Sources[i].References))
                {
                    var sourceName = projectDefinition.Sources[i].Name;
                    var holder = holders[sourceName];

                    for (int j = 0; j < projectDefinition.Sources[i].References.Length; j++)
                    {
                        var referenceDefinition = projectDefinition.Sources[i].References[j];
                        SetReference(sourceName, referenceDefinition.Name, referenceDefinition.ReferenceName);
                    }
                }
            }
        }

        public void ToDefinition(out NoiseProjectDefinition projectDefinition)
        {
            if (RootName == null) throw new InvalidOperationException("RootName is null.");

            projectDefinition = new NoiseProjectDefinition
            {
                RootName = RootName,
                Sources = new NoiseSourceDefinition[holders.Count]
            };

            for (int i = 0; i < holders.Count; i++)
            {
                var holder = holders[i];
                var noiseSource = holder.NoiseSource;
                var noiseSourceInfo = holder.NoiseSourceInfo;

                // Alias
                var typeName = NoiseSourceTypeAlias.GetAlias(noiseSourceInfo.Type);
                // Full name.
                if (typeName == null) typeName = noiseSourceInfo.Type.FullName;

                projectDefinition.Sources[i] = new NoiseSourceDefinition
                {
                    Name = holder.Name,
                    Type = typeName
                };

                projectDefinition.Sources[i].Parameters = new NoiseParameterDefinition[noiseSourceInfo.Parameters.Count];
                for (int j = 0; j < noiseSourceInfo.Parameters.Count; j++)
                {
                    var name = noiseSourceInfo.Parameters[j].Name;
                    var value = noiseSourceInfo.GetParameter(noiseSource, name);

                    projectDefinition.Sources[i].Parameters[j] = new NoiseParameterDefinition
                    {
                        Name = name,
                        Value = value
                    };
                }

                projectDefinition.Sources[i].References = new NoiseReferenceDefinition[noiseSourceInfo.References.Count];
                for (int j = 0; j < noiseSourceInfo.References.Count; j++)
                {
                    var name = noiseSourceInfo.References[j].Name;
                    var reference = noiseSourceInfo.GetReference(noiseSource, name);
                    var referenceName = GetReferenceName(reference);

                    projectDefinition.Sources[i].References[j] = new NoiseReferenceDefinition
                    {
                        Name = name,
                        ReferenceName = referenceName
                    };
                }
            }
        }

        string GetReferenceName(INoiseSource noiseSource)
        {
            foreach (var holder in holders)
            {
                if (holder.NoiseSource == noiseSource) return holder.Name;
            }

            throw new InvalidOperationException("Noise source not found.");
        }
    }
}
