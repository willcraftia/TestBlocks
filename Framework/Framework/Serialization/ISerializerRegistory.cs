#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Serialization
{
    public interface ISerializerRegistory
    {
        ISerializer ResolveSerializer(Uri uri, Type type);
    }
}
