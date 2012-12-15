#region Using

using System;
using Willcraftia.Xna.Framework.Component;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class RandomBiomeManagerCore : CatalogedBiomeManagerCore, IInitializingComponent
    {
        int seed = Environment.TickCount;

        Random random = new Random();

        public int Seed
        {
            get { return seed; }
            set { seed = value; }
        }

        // I/F
        public void Initialize()
        {
            random = new Random(seed);
        }

        protected override byte GetBiomeIndex(Chunk chunk)
        {
            // TODO: 仮想 BiomeBounds の判定

            //return (byte) random.Next(BiomeCatalog.Count);
            throw new NotImplementedException();
        }
    }
}
