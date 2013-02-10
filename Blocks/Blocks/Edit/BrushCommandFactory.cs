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
            pool.Register(typeof(SetBrushCommand), () => { return new SetBrushCommand(); });
        }

        public PickBlockCommand CreatePickBlockCommand()
        {
            return Create(typeof(PickBlockCommand)) as PickBlockCommand;
        }

        public SetBrushCommand CreateSetBrushCommand()
        {
            return Create(typeof(SetBrushCommand)) as SetBrushCommand;
        }

        BrushCommand Create(Type type)
        {
            var command = pool.Borrow(type) as BrushCommand;
            command.BrushManager = brushManager;
            return command;
        }
    }
}
