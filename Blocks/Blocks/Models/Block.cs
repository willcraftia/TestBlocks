#region Using

using System;
using Willcraftia.Xna.Framework.Content;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class Block : IAsset
    {
        public const byte EmptyIndex = 0;

        // MEMO
        //
        // 編集と複製に関して。
        // エディタでは、読み取り専用スキームの Block を編集可、および、複製可とする。
        // 読み取り専用である場合、書き込み可能な URI を指定するように促す。
        // これに拒否する場合、保存は取り消される。

        // I/F
        public IResource Resource { get; set; }

        public byte Index { get; set; }

        public string Name { get; set; }

        public Mesh Mesh { get; set; }

        public Tile TopTile { get; set; }
        
        public Tile BottomTile { get; set; }
        
        public Tile FrontTile { get; set; }
        
        public Tile BackTile { get; set; }
        
        public Tile LeftTile { get; set; }
        
        public Tile RightTile { get; set; }

        public bool Fluid { get; set; }

        public bool ShadowCasting { get; set; }

        public BlockShape Shape { get; set; }

        public float Mass { get; set; }

        // Always immovable.
        //public bool Immovable { get; set; }

        public float StaticFriction { get; set; }

        public float DynamicFriction { get; set; }

        public float Restitution { get; set; }
    }
}
