#region Using

using System;
using System.Diagnostics;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class SetBrushCommand : BrushCommand
    {
        Brush lastBrush;

        public Brush Brush { get; set; }

        internal SetBrushCommand() { }

        public override bool Do()
        {
            if (Brush == null) throw new InvalidOperationException("Brush is null.");

            if (BrushManager.ActiveBrush == Brush) return false;

            lastBrush = BrushManager.ActiveBrush;
            BrushManager.ActiveBrush = Brush;

            return true;
        }

        public override void Undo()
        {
            if (lastBrush == null) throw new InvalidOperationException("Brush is null.");

            BrushManager.ActiveBrush = lastBrush;

            base.Undo();
        }
    }
}
