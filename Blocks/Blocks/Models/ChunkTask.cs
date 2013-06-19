#region Using

using System;
using System.Threading;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public abstract class ChunkTask
    {
        internal delegate void Callback(ChunkTaskRequest request, ChunkTask task);

        internal WaitCallback WaitCallbackMethod;

        Callback callback;

        protected ChunkTaskManager TaskManager { get; private set; }

        protected ChunkTask()
        {
            WaitCallbackMethod = new WaitCallback(WaitCallback);
        }

        internal void Initialize(ChunkTaskManager taskManager, Callback callback)
        {
            if (taskManager == null) throw new ArgumentNullException("taskManager");
            if (callback == null) throw new ArgumentNullException("callback");

            TaskManager = taskManager;
            this.callback = callback;
        }

        internal void WaitCallback(object state)
        {
            var request = state as ChunkTaskRequest;

            Execute(request);
            
            callback(request, this);
        }

        protected abstract void Execute(ChunkTaskRequest request);
    }
}
