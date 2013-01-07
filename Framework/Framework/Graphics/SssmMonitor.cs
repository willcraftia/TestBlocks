#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class SssmMonitor
    {
        public event EventHandler BeginDraw = delegate { };

        public event EventHandler EndDraw = delegate { };

        public event EventHandler BeginFilter = delegate { };

        public event EventHandler EndFilter = delegate { };

        Sssm sssm;

        public SssmMonitor(Sssm sssm)
        {
            if (sssm == null) throw new ArgumentNullException("sssm");

            this.sssm = sssm;
        }

        internal void OnBeginDraw()
        {
            BeginDraw(sssm, EventArgs.Empty);
        }

        internal void OnEndDraw()
        {
            EndDraw(sssm, EventArgs.Empty);
        }

        internal void OnBeginFilter()
        {
            BeginFilter(sssm, EventArgs.Empty);
        }

        internal void OnEndFilter()
        {
            EndFilter(sssm, EventArgs.Empty);
        }
    }
}
