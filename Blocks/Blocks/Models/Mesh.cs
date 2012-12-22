#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Content;
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

        public MeshPart this[CubeSides side]
        {
            get
            {
                switch (side)
                {
                    case CubeSides.Top:
                        return Top;
                    case CubeSides.Bottom:
                        return Bottom;
                    case CubeSides.Front:
                        return Front;
                    case CubeSides.Back:
                        return Back;
                    case CubeSides.Left:
                        return Left;
                    case CubeSides.Right:
                        return Right;
                    default:
                        throw new InvalidOperationException();
                }
            }
            set
            {
                switch (side)
                {
                    case CubeSides.Top:
                        Top = value;
                        break;
                    case CubeSides.Bottom:
                        Bottom = value;
                        break;
                    case CubeSides.Front:
                        Front = value;
                        break;
                    case CubeSides.Back:
                        Back = value;
                        break;
                    case CubeSides.Left:
                        Left = value;
                        break;
                    case CubeSides.Right:
                        Right = value;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        #region ToString

        public override string ToString()
        {
            return "[Uri:" + ((Resource != null) ? Resource.AbsoluteUri : string.Empty) + "]";
        }

        #endregion
    }
}
