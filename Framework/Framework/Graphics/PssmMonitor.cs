#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class PssmMonitor
    {
        Pssm pssm;

        public int TotalShadowCasterCount { get; set; }

        public PssmMonitor(Pssm pssm)
        {
            if (pssm == null) throw new ArgumentNullException("pssm");

            this.pssm = pssm;
        }
    }
}
