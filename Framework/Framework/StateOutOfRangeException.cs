#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework
{
    public sealed class StateOutOfRangeException : Exception
    {
        public StateOutOfRangeException(string message)
            : base(message)
        {
        }
    }
}
