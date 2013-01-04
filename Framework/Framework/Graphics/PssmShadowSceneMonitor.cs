﻿#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class PssmShadowSceneMonitor
    {
        public event EventHandler BeginDraw = delegate { };

        public event EventHandler EndDraw = delegate { };

        PssmShadowScene shadowScene;

        public PssmShadowSceneMonitor(PssmShadowScene shadowScene)
        {
            if (shadowScene == null) throw new ArgumentNullException("shadowScene");

            this.shadowScene = shadowScene;
        }

        internal void OnBeginDraw()
        {
            BeginDraw(shadowScene, EventArgs.Empty);
        }

        internal void OnEndDraw()
        {
            EndDraw(shadowScene, EventArgs.Empty);
        }
    }
}
