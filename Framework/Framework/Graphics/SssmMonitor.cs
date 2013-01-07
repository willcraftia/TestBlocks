#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class SssmMonitor
    {
        public event EventHandler BeginProcess = delegate { };

        public event EventHandler EndProcess = delegate { };

        public event EventHandler BeginDrawShadowScene = delegate { };

        public event EventHandler EndDrawShadowScene = delegate { };

        public event EventHandler BeginFilter = delegate { };

        public event EventHandler EndFilter = delegate { };

        Sssm sssm;

        public SssmMonitor(Sssm sssm)
        {
            if (sssm == null) throw new ArgumentNullException("sssm");

            this.sssm = sssm;
        }

        internal void OnBeginProcess()
        {
            BeginProcess(sssm, EventArgs.Empty);
        }

        internal void OnEndProcess()
        {
            EndProcess(sssm, EventArgs.Empty);
        }

        internal void OnBeginDrawShadowScene()
        {
            BeginDrawShadowScene(sssm, EventArgs.Empty);
        }

        internal void OnEndDrawShadowScene()
        {
            EndDrawShadowScene(sssm, EventArgs.Empty);
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
