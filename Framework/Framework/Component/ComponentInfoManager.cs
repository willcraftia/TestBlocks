#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Willcraftia.Xna.Framework.Component
{
    public sealed class ComponentInfoManager
    {
        #region ComponentInfoCollection

        class ComponentInfoCollection : KeyedCollection<Type, ComponentInfo>
        {
            protected override Type GetKeyForItem(ComponentInfo item)
            {
                return item.ComponentType;
            }
        }

        #endregion

        static readonly TypeHandler defaultTypeHandler = new TypeHandler();

        ComponentTypeRegistory typeRegistory;

        Dictionary<Type, ITypeHandler> typeHandlerMap;

        ComponentInfoCollection componentInfoCache = new ComponentInfoCollection();

        public ComponentInfoManager(ComponentTypeRegistory typeRegistory)
        {
            if (typeRegistory == null) throw new ArgumentNullException("typeRegistory");

            this.typeRegistory = typeRegistory;
        }

        public void AddTypeHandler(Type type, ITypeHandler typeHandler)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (typeHandler == null) throw new ArgumentNullException("typeHandler");

            if (typeHandlerMap == null) typeHandlerMap = new Dictionary<Type, ITypeHandler>();
            typeHandlerMap[type] = typeHandler;
        }

        public ComponentInfo GetComponentInfo(string typeName)
        {
            if (typeName == null) throw new ArgumentNullException("typeName");

            var type = typeRegistory.GetType(typeName);
            return GetComponentInfo(type);
        }

        public ComponentInfo GetComponentInfo(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            if (componentInfoCache.Contains(type)) return componentInfoCache[type];

            var typeHandler = GetTypeHandler(type);
            var componentInfo = new ComponentInfo(type, typeHandler);
            componentInfoCache.Add(componentInfo);
            return componentInfo;
        }

        public string GetTypeDefinitionName(ComponentInfo componentInfo)
        {
            return typeRegistory.GetTypeDefinitionName(componentInfo.ComponentType);
        }

        ITypeHandler GetTypeHandler(Type type)
        {
            ITypeHandler typeHandler;
            if (typeHandlerMap == null || !typeHandlerMap.TryGetValue(type, out typeHandler))
                typeHandler = defaultTypeHandler;

            return typeHandler;
        }
    }
}
