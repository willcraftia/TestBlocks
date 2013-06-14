#region Using

using System;
using System.Collections.Generic;
using System.Threading;

#endregion

namespace Willcraftia.Xna.Framework.Threading
{
    public sealed class TaskQueue
    {
        #region Task

        struct Task
        {
            public Action TaskWithoutState;

            public Action<object> TaskWithState;

            public object State;

            public void Execute()
            {
                if (TaskWithoutState != null)
                {
                    TaskWithoutState();
                }
                else
                {
                    TaskWithState(State);
                }
            }

            public void Clear()
            {
                TaskWithoutState = null;
                TaskWithState = null;
                State = null;
            }
        }

        #endregion

        #region Slot

        class Slot
        {
            public Task Task;
        }

        #endregion

        public const int DefaultConcurrencyLevel = 10;

        int concurrencyLevel = DefaultConcurrencyLevel;

        Queue<Task> queue = new Queue<Task>();

        Stack<Slot> freeSlots = new Stack<Slot>();

        Dictionary<Slot, Slot> activeSlots = new Dictionary<Slot, Slot>();

        WaitCallback waitCallbackDelegate;

        public int ConcurrencyLevel
        {
            get { return concurrencyLevel; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                concurrencyLevel = value;
            }
        }

        public int QueueCount
        {
            get { return queue.Count; }
        }

        public int FreeSlotCount
        {
            get { lock (freeSlots) return freeSlots.Count; }
        }

        public int ActiveSlotCount
        {
            get { lock (activeSlots) return activeSlots.Count; }
        }

        public TaskQueue(int concurrencyLevel = DefaultConcurrencyLevel)
        {
            this.concurrencyLevel = concurrencyLevel;
            waitCallbackDelegate = WaitCallback;
        }

        public void Enqueue(Action action)
        {
            var task = new Task
            {
                TaskWithoutState = action
            };

            queue.Enqueue(task);
        }

        public void Enqueue(Action<object> action, object state)
        {
            var task = new Task
            {
                TaskWithState = action,
                State = state
            };

            queue.Enqueue(task);
        }

        public void Update()
        {
            // キューが空ならば即座に終了。
            if (queue.Count == 0) return;

            // スロット数の調整。
            AdjustSlots();

            while (0 < queue.Count)
            {
                Slot slot;
                lock (freeSlots)
                {
                    lock (activeSlots)
                    {
                        // 空きスロットが無いならば処理終了。
                        if (concurrencyLevel < freeSlots.Count + activeSlots.Count)
                            return;

                        // 空きスロットの取得。
                        if (0 < freeSlots.Count)
                        {
                            slot = freeSlots.Pop();
                        }
                        else
                        {
                            slot = new Slot();
                        }

                        // スロットを利用中としてマーク。
                        activeSlots[slot] = slot;
                    }
                }

                // タスクを取得してスロットへ関連付け。
                slot.Task = queue.Dequeue();

                // スロットへスレッドを割り当て。
                ThreadPool.QueueUserWorkItem(waitCallbackDelegate, slot);
            }
        }

        public void Clear()
        {
            queue.Clear();
        }

        void WaitCallback(object state)
        {
            var slot = state as Slot;

            // タスク実行。
            slot.Task.Execute();

            // スロットからタスクを開放。
            slot.Task.Clear();

            lock (freeSlots)
            {
                lock (activeSlots)
                {
                    // 空きスロットとしてマーク。
                    activeSlots.Remove(slot);
                    freeSlots.Push(slot);
                }
            }
        }

        void AdjustSlots()
        {
            lock (freeSlots)
            {
                lock (activeSlots)
                {
                    while (concurrencyLevel < freeSlots.Count + activeSlots.Count && 0 < freeSlots.Count)
                        freeSlots.Pop();
                }
            }
        }
    }
}
