#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Serialization
{
    public interface ISerializerRegistory
    {
        ISerializer ResolveSerializer(IUri uri, Type type);
    }
}
