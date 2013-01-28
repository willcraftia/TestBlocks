#region Using

using System;
using System.Collections.Generic;
using Willcraftia.Xna.Framework;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class WorldEditManager
    {
        #region Command

        abstract class Command
        {
            public readonly LinkedListNode<Command> Node;

            protected WorldEditManager Owner { get; private set; }

            protected Command(WorldEditManager owner)
            {
                if (owner == null) throw new ArgumentNullException("owner");

                Node = new LinkedListNode<Command>(this);
                Owner = owner;
            }

            public abstract void Do();

            public abstract void Undo();

            public abstract void Return();
        }

        #endregion

        #region SetBlockCommand

        sealed class SetBlockCommand : Command
        {
            public VectorI3 BlockPosition;

            public byte BlockIndex;

            public byte LastBlockIndex;

            public SetBlockCommand(WorldEditManager owner) : base(owner) { }

            public override void Do()
            {
                var chunkManager = Owner.worldManager.ChunkManager;
                var chunk = chunkManager.GetChunkByBlockPosition(BlockPosition);
                if (chunk == null) throw new InvalidOperationException("Chunk not found: BlockPosition=" + BlockPosition);

                var relativePosition = chunk.GetRelativeBlockPosition(BlockPosition);

                LastBlockIndex = chunk[relativePosition];

                chunk[relativePosition] = BlockIndex;
            }

            public override void Undo()
            {
                var chunkManager = Owner.worldManager.ChunkManager;
                var chunk = chunkManager.GetChunkByBlockPosition(BlockPosition);
                if (chunk == null) throw new InvalidOperationException("Chunk not found: BlockPosition=" + BlockPosition);

                var relativePosition = chunk.GetRelativeBlockPosition(BlockPosition);

                chunk[relativePosition] = LastBlockIndex;
            }

            public override void Return()
            {
                Owner.setBlockCommandPool.Return(this);
            }
        }

        #endregion

        #region UndoCommand

        sealed class UndoCommand : Command
        {
            public UndoCommand(WorldEditManager owner) : base(owner) { }

            public override void Do()
            {
                if (Owner.UndoStackCount == 0) return;

                var node = Owner.PopUndoStack();
                node.Value.Undo();

                Owner.PushRedoStack(node);
            }

            public override void Undo() { }

            public override void Return()
            {
                Owner.undoCommandPool.Return(this);
            }
        }

        #endregion

        #region RedoCommand

        sealed class RedoCommand : Command
        {
            public RedoCommand(WorldEditManager owner) : base(owner) { }

            public override void Do()
            {
                if (Owner.RedoStackCount == 0) return;

                var node = Owner.PopRedoStack();
                node.Value.Do();

                Owner.PushUndoStack(node);
            }

            public override void Undo() { }

            public override void Return()
            {
                Owner.redoCommandPool.Return(this);
            }
        }

        #endregion

        WorldManager worldManager;

        ConcurrentPool<SetBlockCommand> setBlockCommandPool;

        ConcurrentPool<UndoCommand> undoCommandPool;

        ConcurrentPool<RedoCommand> redoCommandPool;

        LinkedList<Command> commandQueue;

        LinkedList<Command> undoStack;

        LinkedList<Command> redoStack;

        int undoCapacity = 100;

        int redoCapacity = 100;

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

        public WorldEditManager(WorldManager worldManager)
        {
            if (worldManager == null) throw new ArgumentNullException("worldManager");

            this.worldManager = worldManager;

            setBlockCommandPool = new ConcurrentPool<SetBlockCommand>(() => { return new SetBlockCommand(this); });
            undoCommandPool = new ConcurrentPool<UndoCommand>(() => { return new UndoCommand(this); });
            redoCommandPool = new ConcurrentPool<RedoCommand>(() => { return new RedoCommand(this); });

            commandQueue = new LinkedList<Command>();
            undoStack = new LinkedList<Command>();
            redoStack = new LinkedList<Command>();
        }

        public void RequestSetBlock(VectorI3 blockPosition, byte blockIndex)
        {
            var command = setBlockCommandPool.Borrow();
            command.BlockPosition = blockPosition;
            command.BlockIndex = blockIndex;

            Enqueue(command.Node);
        }

        public void RequestUndo()
        {
            Enqueue(undoCommandPool.Borrow().Node);
        }

        public void RequestRedo()
        {
            Enqueue(redoCommandPool.Borrow().Node);
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
                oldestNode.Value.Return();
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
                oldestNode.Value.Return();
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
                node.Value.Return();
            }
        }

        void ClearRedoStack()
        {
            while (0 < redoStack.Count)
            {
                var node = redoStack.First;
                redoStack.RemoveFirst();

                // プールへ戻す。
                node.Value.Return();
            }
        }

        void ClearCommandQueue()
        {
            while (0 < commandQueue.Count)
            {
                var node = commandQueue.First;
                commandQueue.RemoveFirst();

                // プールへ戻す。
                node.Value.Return();
            }
        }
    }
}
