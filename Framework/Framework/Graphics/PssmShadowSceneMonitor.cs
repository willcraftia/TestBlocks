#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class PssmShadowSceneMonitor
    {
        public event EventHandler BeginDraw = delegate { };

        public event EventHandler EndDraw = delegate { };

        PssmShadowScene pssmShadowScene;

        public PssmShadowSceneMonitor(PssmShadowScene pssmShadowScene)
        {
            if (pssmShadowScene == null) throw new ArgumentNullException("pssmShadowScene");

            this.pssmShadowScene = pssmShadowScene;
        }

        internal void OnBeginDraw()
        {
            BeginDraw(pssmShadowScene, EventArgs.Empty);
        }

        internal void OnEndDraw()
        {
            EndDraw(pssmShadowScene, EventArgs.Empty);
        }
    }
}
