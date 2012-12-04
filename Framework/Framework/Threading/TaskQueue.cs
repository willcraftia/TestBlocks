#region Using

using System;
using System.Collections.Generic;
using System.Threading;

#endregion

namespace Willcraftia.Xna.Framework.Threading
{
    public sealed class TaskQueue
    {
        #region Slot

        class Slot
        {
            public Action Task { get; set; }
        }

        #endregion

        public const int MaxSlotCount = 20;

        public const int DefaultSlotCount = 4;

        int slotCount = DefaultSlotCount;

        Queue<Action> queue = new Queue<Action>();

        Stack<Slot> freeSlots = new Stack<Slot>();

        List<Slot> activeSlots = new List<Slot>();

        public int SlotCount
        {
            get { return slotCount; }
            set
            {
                if (slotCount < 1 || MaxSlotCount < slotCount)
                    throw new InvalidOperationException("SlotCount < 1 || MaxSlotCount < SlotCount");

                slotCount = value;
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

        public void Enqueue(Action action)
        {
            queue.Enqueue(action);
        }

        public void Update()
        {
            //
            // Process one partition per frame.
            //

            // Pre-check
            if (queue.Count == 0) return;

            Slot slot;
            lock (freeSlots)
            {
                lock (activeSlots)
                {
                    while (slotCount < freeSlots.Count + activeSlots.Count && 0 < freeSlots.Count)
                        freeSlots.Pop();

                    if (slotCount < freeSlots.Count + activeSlots.Count)
                        return;

                    // Get a free slot.
                    if (0 < freeSlots.Count)
                    {
                        slot = freeSlots.Pop();
                    }
                    else
                    {
                        slot = new Slot();
                    }

                    // Activate this slot.
                    activeSlots.Add(slot);
                }
            }

            // Dequeue a task and assign it into the free slot.
            slot.Task = queue.Dequeue();

            // Assign a thread into this slot.
            ThreadPool.QueueUserWorkItem(WaitCallback, slot);
        }

        public void Clear()
        {
            queue.Clear();
        }

        void WaitCallback(object state)
        {
            var slot = state as Slot;

            slot.Task();

            // Free this slot.
            slot.Task = null;
            lock (freeSlots)
            {
                lock (activeSlots)
                {
                    // Deactivate this slot.
                    activeSlots.Remove(slot);

                    // Adjust the number of slots.
                    if (slotCount < freeSlots.Count + activeSlots.Count)
                        return;

                    // Free this slot.
                    freeSlots.Push(slot);
                }
            }
        }
    }
}
