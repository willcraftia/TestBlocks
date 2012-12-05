#region Using

using System;
using System.IO;

#endregion

namespace Willcraftia.Xna.Framework.Serialization
{
    public interface ISerializer
    {
        bool CanDeserializeIntoExistingObject { get; }

        T Deserialize<T>(Stream stream);

        T Deserialize<T>(Stream stream, T existingInstance) where T : class;

        object Deserialize(Stream stream, Type type, object existingInstance);

        void Serialize(Stream stream, object resource);
    }
}
