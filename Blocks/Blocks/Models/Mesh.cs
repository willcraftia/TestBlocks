#region Using

using System;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class Mesh : IAsset
    {
        // I/F
        public IResource Resource { get; set; }

        public string Name { get; set; }

        public MeshPart Top { get; set; }

        public MeshPart Bottom { get; set; }

        public MeshPart Front { get; set; }

        public MeshPart Back { get; set; }

        public MeshPart Left { get; set; }

        public MeshPart Right { get; set; }

        public MeshPart this[Side side]
        {
            get
            {
                switch (side)
                {
                    case Side.Top:
                        return Top;
                    case Side.Bottom:
                        return Bottom;
                    case Side.Front:
                        return Front;
                    case Side.Back:
                        return Back;
                    case Side.Left:
                        return Left;
                    case Side.Right:
                        return Right;
                    default:
                        throw new InvalidOperationException();
                }
            }
            set
            {
                switch (side)
                {
                    case Side.Top:
                        Top = value;
                        break;
                    case Side.Bottom:
                        Bottom = value;
                        break;
                    case Side.Front:
                        Front = value;
                        break;
                    case Side.Back:
                        Back = value;
                        break;
                    case Side.Left:
                        Left = value;
                        break;
                    case Side.Right:
                        Right = value;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }
}
