#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class PssmSceneMonitor
    {
        public event EventHandler BeginDraw = delegate { };

        public event EventHandler EndDraw = delegate { };

        PssmScene pssmScene;

        public PssmSceneMonitor(PssmScene pssmScene)
        {
            if (pssmScene == null) throw new ArgumentNullException("pssmScene");

            this.pssmScene = pssmScene;
        }

        internal void OnBeginDraw()
        {
            BeginDraw(pssmScene, EventArgs.Empty);
        }

        internal void OnEndDraw()
        {
            EndDraw(pssmScene, EventArgs.Empty);
        }
    }
}
