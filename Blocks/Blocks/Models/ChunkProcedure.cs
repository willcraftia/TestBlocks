#region Using

using System;
using Willcraftia.Xna.Framework.Component;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkProcedure
    {
        public const string ComponentName = "ChunkProcedure";

        static readonly AliasTypeRegistory typeRegistory = new AliasTypeRegistory();

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
            typeRegistory.SetTypeAlias(typeof(FlatTerrainProcedureComponent), "FlatTerrain");
        }

        public ChunkProcedure()
        {
            ComponentFactory = new ComponentFactory(typeRegistory);
        }

        public void Generate(Chunk chunk)
        {
        }
    }
}
