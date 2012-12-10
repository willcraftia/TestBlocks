#region Using

using System;
using System.IO;

#endregion

namespace Willcraftia.Xna.Framework.Serialization
{
    public interface ISerializer
    {
        bool CanDeserializeIntoExistingObject { get; }

        object Deserialize(Stream stream, object existingInstance);

        void Serialize(Stream stream, object instance);
    }
}
