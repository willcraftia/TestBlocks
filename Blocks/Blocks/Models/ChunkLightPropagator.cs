#region Using

using System;
using System.Diagnostics;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkLightPropagator
    {
        ChunkManager manager;

        public volatile bool Completed;

        public Chunk Chunk { get; internal set; }

        public Action ExecuteAction { get; private set; }

        public ChunkLightPropagator(ChunkManager manager)
        {
            if (manager == null) throw new ArgumentNullException("manager");

            this.manager = manager;
            ExecuteAction = new Action(Execute);
        }

        public void Execute()
        {
            Debug.Assert(!Completed);
            Debug.Assert(Chunk != null);
            Debug.Assert(0 < Chunk.SolidCount);

            Completed = true;
        }
    }
}
