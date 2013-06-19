#region Using

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using Willcraftia.Xna.Framework.Collections;

#endregion

namespace Willcraftia.Xna.Blocks.Models
{
    public sealed class ChunkTaskManager
    {
        class ChunkTaskRequestComparer : IComparer<ChunkTaskRequest>
        {
            public int Compare(ChunkTaskRequest x, ChunkTaskRequest y)
            {
                var byPriority = x.Priority.CompareTo(y.Priority);
                if (byPriority != 0)
                    return byPriority;

                return x.Timestamp.CompareTo(y.Timestamp);
            }
        }

        ConcurrentPool<ChunkTaskRequest> requestPool;

        ConcurrentPriorityQueue<ChunkTaskRequest> requests;

        ChunkTask.Callback taskCallbackMethod;

        Dictionary<ChunkTaskType, ChunkTask> tasks = new Dictionary<ChunkTaskType, ChunkTask>();

        Dictionary<ChunkTaskRequest, ChunkTaskRequest> activeRequests;

        int concurrencyLevel = 10;

        public ChunkTaskManager()
        {
            requestPool = new ConcurrentPool<ChunkTaskRequest>(() => { return new ChunkTaskRequest(); });
            requests = new ConcurrentPriorityQueue<ChunkTaskRequest>(new ChunkTaskRequestComparer());
            taskCallbackMethod = new ChunkTask.Callback(TaskCallback);
            activeRequests = new Dictionary<ChunkTaskRequest, ChunkTaskRequest>(concurrencyLevel);
        }

        public void RegisterTask(ChunkTaskType taskType, ChunkTask task)
        {
            if (task == null) throw new ArgumentNullException("task");

            task.Initialize(this, taskCallbackMethod);
            tasks[taskType] = task;
        }

        public void RequestTask(Chunk chunk, ChunkTaskType taskType, ChunkTaskPriority priority)
        {
            var request = requestPool.Borrow();
            request.Initialize(chunk, taskType, priority);

            requests.Enqueue(request);
        }

        public void Update()
        {
            ChunkTaskRequest request;
            while (activeRequests.Count < concurrencyLevel && requests.TryDequeue(out request))
            {
                var task = tasks[request.TaskType];

                ThreadPool.QueueUserWorkItem(task.WaitCallbackMethod, request);

                lock (activeRequests)
                {
                    activeRequests[request] = request;
                }
            }
        }

        void TaskCallback(ChunkTaskRequest request, ChunkTask task)
        {
            request.Clear();
            requestPool.Return(request);

            lock (activeRequests)
            {
                activeRequests.Remove(request);
            }
        }
    }
}
