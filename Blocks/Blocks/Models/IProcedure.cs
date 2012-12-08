#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public interface IProcedure<T>
    {
        void Generate(T instance);
    }
}
