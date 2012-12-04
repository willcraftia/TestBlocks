#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class Block
    {
        public const byte EmptyIndex = 0;

        // MEMO
        //
        // 編集と複製に関して。
        // エディタでは、読み取り専用スキームの Block を編集可、および、複製可とする。
        // 保存時に、AssetManager.IsReadOnlyScheme でスキームを確認し、
        // 読み取り専用である場合、書き込み可能な URI を指定するように促す。
        // これに拒否する場合、保存は取り消される。

        // Block に限らず、アセットの持つ URI は、IAssetLoader 内でのみ設定する。
        // その他のクラスから URI を直接変更してはならない。
        // アセットの保存では、IAssetLoader は、まず、URI を参照し、
        // これが保存要求の URI と異なる場合、AssetManager 内での URI との関連付けを破棄する。
        // その後、保存要求の URI で保存を行い、アセットにその URI を設定する。

        public Uri Uri { get; set; }

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

        // 厳密に読み取り専用かどうかは、compact framework for xna では保存時まで分からない。
        // Uri スキームから、ある程度は判断できるが、これは Save() の呼び出し側で Uri から事前に判断させる。
        // また、Save() での対象が読み取り専用ファイルである場合は IOException が発生すると思われるが、
        // これも呼び出し側で判断させる。

        //public bool ReadOnly { get; private set; }

        public Tile GetTile(Side side)
        {
            switch (side)
            {
                case Side.Top:
                    return TopTile;
                case Side.Bottom:
                    return BottomTile;
                case Side.Front:
                    return FrontTile;
                case Side.Back:
                    return BackTile;
                case Side.Left:
                    return LeftTile;
                case Side.Right:
                    return RightTile;
                default:
                    throw new InvalidOperationException();
            }
        }

        public void SetTile(Side side, Tile tile)
        {
            switch (side)
            {
                case Side.Top:
                    TopTile = tile;
                    break;
                case Side.Bottom:
                    BottomTile = tile;
                    break;
                case Side.Front:
                    FrontTile = tile;
                    break;
                case Side.Back:
                    BackTile = tile;
                    break;
                case Side.Left:
                    LeftTile = tile;
                    break;
                case Side.Right:
                    RightTile = tile;
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
