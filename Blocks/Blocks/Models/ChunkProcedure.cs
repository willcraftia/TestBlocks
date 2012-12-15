#region Using

using System;
using Willcraftia.Xna.Framework.Component;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkProcedure
    {
        public const string ComponentName = "Target";

        static readonly ComponentTypeRegistory componentTypeRegistory = new ComponentTypeRegistory();

        public ComponentFactory ComponentFactory { get; private set; }

        public ChunkProcedureComponent Component { get; set; }

        public Region Region { get; set; }

        static ChunkProcedure()
        {
            componentTypeRegistory.SetTypeDefinitionName(typeof(FlatTerrainProcedureComponent), "FlatTerrain");
        }

        public ChunkProcedure()
        {
            ComponentFactory = new ComponentFactory(componentTypeRegistory);
        }

        public void Generate(Chunk chunk)
        {
        }
    }
}
