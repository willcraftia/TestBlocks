#region Using

using System;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class BrushCommandFactory
    {
        BrushManager brushManager;

        CommandPool pool = new CommandPool();

        public BrushCommandFactory(BrushManager brushManager)
        {
            if (brushManager == null) throw new ArgumentNullException("brushManager");

            this.brushManager = brushManager;

            pool.Register(typeof(PickBlockCommand), () => { return new PickBlockCommand(); });
        }

        public PickBlockCommand CreatePickBlockCommand()
        {
            return Create(typeof(PickBlockCommand)) as PickBlockCommand;
        }

        BrushCommand Create(Type type)
        {
            var command = pool.Borrow(type) as BrushCommand;
            command.BrushManager = brushManager;
            return command;
        }
    }
}
