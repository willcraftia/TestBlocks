#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Diagnostics
{
    public interface IInstrumentListener
    {
        void Begin(string name);

        void End(string name);

        void Close();
    }
}
