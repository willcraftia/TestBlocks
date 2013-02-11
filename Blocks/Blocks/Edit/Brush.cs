#region Using

using System;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public abstract class Brush
    {
        public VectorI3 Position;

        public VectorI3 PaintPosition;

        public VectorI3 ErasePosition;

        bool active;

        public SceneNode Node { get; private set; }

        public bool Active
        {
            get { return active; }
            set
            {
                if (active == value) return;

                active = value;
                OnActiveChanged();
            }
        }

        public bool PaintStarted { get; private set; }

        public bool CanPaint { get; protected set; }

        protected BrushManager Manager { get; private set; }

        protected Brush(BrushManager manager, SceneNode node)
        {
            if (manager == null) throw new ArgumentNullException("manager");
            if (node == null) throw new ArgumentNullException("node");

            Manager = manager;
            Node = node;
        }

        public virtual void StartPaint()
        {
            PaintStarted = true;
        }

        public virtual void EndPaint()
        {
            PaintStarted = false;
        }

        public abstract void Update(ICamera camera);

        protected virtual void OnActiveChanged()
        {
            Node.SetVisible(active);

            if (!active)
            {
                // 非アクティブになった場合はペイント開始状態をリセット。
                PaintStarted = false;
            }
        }
    }
}
