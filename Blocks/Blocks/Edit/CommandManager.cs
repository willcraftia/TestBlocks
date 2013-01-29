#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Blocks.Edit
{
    public sealed class CommandManager
    {
        #region UndoCommand

        sealed class UndoCommand : Command
        {
            CommandManager owner;

            public UndoCommand(CommandManager owner)
            {
                this.owner = owner;
            }

            public override void Do()
            {
                if (owner.UndoStackCount == 0) return;

                var node = owner.PopUndoStack();
                node.Value.Undo();

                owner.PushRedoStack(node);
            }

            public override void Undo() { }

            public override void Release() { }
        }

        #endregion

        #region RedoCommand

        sealed class RedoCommand : Command
        {
            CommandManager owner;

            public RedoCommand(CommandManager owner)
            {
                this.owner = owner;
            }

            public override void Do()
            {
                if (owner.RedoStackCount == 0) return;

                var node = owner.PopRedoStack();
                node.Value.Do();

                owner.PushUndoStack(node);
            }

            public override void Undo() { }

            public override void Release() { }
        }

        #endregion

        LinkedList<Command> commandQueue;

        LinkedList<Command> undoStack;

        LinkedList<Command> redoStack;

        int undoCapacity = 100;

        int redoCapacity = 100;

        UndoCommand undoCommand;

        RedoCommand redoCommand;

        public int CommandQueueCount
        {
            get { return commandQueue.Count; }
        }

        public int UndoStackCount
        {
            get { return undoStack.Count; }
        }

        public int RedoStackCount
        {
            get { return redoStack.Count; }
        }

        public CommandManager()
        {
            commandQueue = new LinkedList<Command>();
            undoStack = new LinkedList<Command>();
            redoStack = new LinkedList<Command>();

            undoCommand = new UndoCommand(this);
            redoCommand = new RedoCommand(this);
        }

        public void RequestCommand(Command command)
        {
            Enqueue(command.Node);
        }

        public void RequestUndo()
        {
            RequestCommand(undoCommand);
        }

        public void RequestRedo()
        {
            RequestCommand(redoCommand);
        }

        public void Update()
        {
            while (0 < commandQueue.Count)
            {
                // 先頭ノードを取得。
                LinkedListNode<Command> node;
                if (!TryDequeue(out node)) break;

                // コマンドを実行。
                var command = node.Value;
                command.Do();

                if (!(command is UndoCommand) && !(command is RedoCommand))
                {
                    // Undo 履歴へ追加。
                    PushUndoStack(node);

                    // 新たなコマンドが実行されたならば Redo 履歴を全て消去。
                    ClearRedoStack();
                }
            }
        }

        void Enqueue(LinkedListNode<Command> commandNode)
        {
            lock (commandQueue)
            {
                commandQueue.AddLast(commandNode);
            }
        }

        bool TryDequeue(out LinkedListNode<Command> result)
        {
            lock (commandQueue)
            {
                if (commandQueue.Count == 0)
                {
                    result = null;
                    return false;
                }

                result = commandQueue.First;
                commandQueue.RemoveFirst();
                return true;
            }
        }

        void PushUndoStack(LinkedListNode<Command> commandNode)
        {
            if (undoStack.Count == undoCapacity)
            {
                // Undo 履歴上限に達しているならば最古の履歴を削除。
                var oldestNode = undoStack.First;
                undoStack.RemoveFirst();

                // プールへ戻す。
                oldestNode.Value.Release();
            }

            undoStack.AddLast(commandNode);
        }

        LinkedListNode<Command> PopUndoStack()
        {
            var topNode = undoStack.Last;
            undoStack.RemoveLast();

            return topNode;
        }

        void PushRedoStack(LinkedListNode<Command> commandNode)
        {
            if (redoStack.Count == redoCapacity)
            {
                // Redo 履歴上限に達しているならば最古の履歴を削除。
                var oldestNode = redoStack.First;
                redoStack.RemoveFirst();

                // プールへ戻す。
                oldestNode.Value.Release();
            }

            redoStack.AddLast(commandNode);
        }

        LinkedListNode<Command> PopRedoStack()
        {
            var topNode = redoStack.Last;
            redoStack.RemoveLast();

            return topNode;
        }

        void ClearUndoStack()
        {
            while (0 < undoStack.Count)
            {
                var node = undoStack.First;
                undoStack.RemoveFirst();

                // プールへ戻す。
                node.Value.Release();
            }
        }

        void ClearRedoStack()
        {
            while (0 < redoStack.Count)
            {
                var node = redoStack.First;
                redoStack.RemoveFirst();

                // プールへ戻す。
                node.Value.Release();
            }
        }

        void ClearCommandQueue()
        {
            while (0 < commandQueue.Count)
            {
                var node = commandQueue.First;
                commandQueue.RemoveFirst();

                // プールへ戻す。
                node.Value.Release();
            }
        }
    }
}
