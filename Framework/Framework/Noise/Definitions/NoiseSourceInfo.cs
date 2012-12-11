#region Using

using System;
using System.Collections.ObjectModel;
using System.Reflection;

#endregion

namespace Willcraftia.Xna.Framework.Noise.Definitions
{
    public sealed class NoiseSourceInfo
    {
        NoisePropertyInfoCollection parameters;

        NoisePropertyInfoCollection references;

        public Type Type { get; private set; }

        public ReadOnlyCollection<PropertyInfo> Parameters
        {
            get { return parameters.AsReadOnly(); }
        }

        public ReadOnlyCollection<PropertyInfo> References
        {
            get { return references.AsReadOnly(); }
        }

        internal NoiseSourceInfo(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (!typeof(INoiseSource).IsAssignableFrom(type))
                throw new ArgumentException("Invalid type: " + type.FullName);

            Type = type;
            parameters = new NoisePropertyInfoCollection();
            references = new NoisePropertyInfoCollection();

            foreach (var propertyInfo in type.GetProperties())
            {
                if (IsParameterProperty(propertyInfo))
                {
                    parameters.Add(propertyInfo);
                }
                else if (IsReferenceProperty(propertyInfo))
                {
                    references.Add(propertyInfo);
                }
            }
        }

        public INoiseSource CreateInstance()
        {
            return Type.InvokeMember(null, BindingFlags.CreateInstance, null, null, null) as INoiseSource;
        }

        public bool ParameterExists(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return parameters.Contains(name);
        }

        public void SetParameter(INoiseSource instance, string name, object value)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (name == null) throw new ArgumentNullException("name");

            var propertyInfo = GetParameterPropertyInfo(name);
            propertyInfo.SetValue(instance, value, null);
        }

        public object GetParameter(INoiseSource instance, string name)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (name == null) throw new ArgumentNullException("name");

            var propertyInfo = GetParameterPropertyInfo(name);
            return propertyInfo.GetValue(instance, null);
        }

        public bool ReferenceExists(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return references.Contains(name);
        }

        public void SetReference(INoiseSource instance, string name, INoiseSource reference)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (name == null) throw new ArgumentNullException("name");

            var propertyInfo = GetReferencePropertyInfo(name);
            propertyInfo.SetValue(instance, reference, null);
        }

        public INoiseSource GetReference(INoiseSource instance, string name)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (name == null) throw new ArgumentNullException("name");

            var propertyInfo = GetReferencePropertyInfo(name);
            return propertyInfo.GetValue(instance, null) as INoiseSource;
        }

        public void UnbindReference(INoiseSource instance, INoiseSource reference)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (reference == null) throw new ArgumentNullException("reference");

            foreach (var propertyInfo in references)
            {
                var referencedSource = propertyInfo.GetValue(instance, null);
                if (referencedSource == reference) propertyInfo.SetValue(instance, null, null);
            }
        }

        bool IsParameterProperty(PropertyInfo propertyInfo)
        {
            return HasAttribute(propertyInfo, typeof(NoiseParameterAttribute)) &&
                propertyInfo.CanRead && propertyInfo.CanWrite;
        }

        bool IsReferenceProperty(PropertyInfo propertyInfo)
        {
            return HasAttribute(propertyInfo, typeof(NoiseReferenceAttribute)) &&
                propertyInfo.CanRead && propertyInfo.CanWrite;
        }

        bool HasAttribute(PropertyInfo propertyInfo, Type attributeType)
        {
            return Attribute.GetCustomAttribute(propertyInfo, attributeType, true) != null;
        }

        PropertyInfo GetParameterPropertyInfo(string name)
        {
            if (!ParameterExists(name)) throw new InvalidOperationException("Parameter not found: " + name);

            return parameters[name];
        }

        PropertyInfo GetReferencePropertyInfo(string name)
        {
            if (!ReferenceExists(name)) throw new InvalidOperationException("Referece not found: " + name);

            return references[name];
        }
    }
}
