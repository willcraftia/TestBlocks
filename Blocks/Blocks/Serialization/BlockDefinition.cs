#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Serialization
{
    public sealed class BlockDefinition
    {
        //----------------------------
        // Editor/Debug

        public string Name;

        //----------------------------
        // Representation

        public string Mesh;

        public string TopTile;

        public string BottomTile;

        public string FrontTile;

        public string BackTile;

        public string LeftTile;

        public string RightTile;

        //----------------------------
        // Behavior

        public bool Fluid;

        public bool ShadowCasting;

        //----------------------------
        // Physics

        // As a collision shape.
        public BlockShape Shape;

        public float Mass;

        // Always immovable.
        //public bool Immovable;

        public float StaticFriction;

        public float DynamicFriction;

        public float Restitution;
    }
}
