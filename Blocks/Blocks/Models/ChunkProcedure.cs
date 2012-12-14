#region Using

using System;
using Willcraftia.Xna.Framework.Component;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkProcedure
    {
        public const string ComponentName = "ChunkProcedure";

        static readonly ComponentTypeRegistory componentTypeRegistory = new ComponentTypeRegistory();

        ChunkProcedureComponent component;

        public ComponentFactory ComponentFactory { get; private set; }

        public ChunkProcedureComponent Component
        {
            get
            {
                if (component == null)
                    component = ComponentFactory[ComponentName] as ChunkProcedureComponent;
                return component;
            }
        }

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
