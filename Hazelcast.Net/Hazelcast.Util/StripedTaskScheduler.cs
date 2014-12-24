using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Util
{
    internal class StripedTaskScheduler : TaskScheduler, IDisposable
    {
        private readonly List<Thread> _threads;
        private IDictionary<int,BlockingCollection<Task>> _tasks;

        private readonly int _numberOfThreads;

        public StripedTaskScheduler(int numberOfThreads)
        {
            if (numberOfThreads < 1) throw new ArgumentOutOfRangeException("numberOfThreads");

            _numberOfThreads = numberOfThreads;
            _tasks = new Dictionary<int, BlockingCollection<Task>>();
            for (int i = 0; i < numberOfThreads; i++)
            {
                _tasks.Add(i, new BlockingCollection<Task>());
            }

            // Create the threads to be used by this scheduler
            _threads = Enumerable.Range(0, numberOfThreads).Select(i =>
            {
                var thread = new Thread(()=> ThreadLoop(i))
                {IsBackground = true};
                return thread;
            }).ToList();

            // Start all of the threads
            _threads.ForEach(t => t.Start());
        }

        private void ThreadLoop(int threadId)
        {
            BlockingCollection<Task> blockingTasks;
            _tasks.TryGetValue(threadId, out blockingTasks);
            if(blockingTasks == null) return;

            while (!blockingTasks.IsAddingCompleted)
            {
                var task = blockingTasks.Take();
                TryExecuteTask(task);
            }
        }

        /// <summary>Gets the maximum concurrency level supported by this scheduler.</summary>
        public override int MaximumConcurrencyLevel
        {
            get { return _numberOfThreads; }
        }

        /// <summary>
        ///     Cleans up the scheduler by indicating that no more tasks will be queued.
        ///     This method blocks until all threads successfully shutdown.
        /// </summary>
        public void Dispose()
        {
            if (_tasks != null)
            {
                // Indicate that no new tasks will be coming in
                foreach (var task in _tasks)
                {
                    task.Value.CompleteAdding();
                }

                // Wait for all threads to finish processing tasks
                foreach (Thread thread in _threads) thread.Join();

                // Cleanup
                foreach (var task in _tasks)
                {
                    task.Value.Dispose();
                }
                _tasks = null;
            }
        }

        /// <summary>Queues a Task to be executed by this scheduler.</summary>
        /// <param name="task">The task to be executed.</param>
        protected override void QueueTask(Task task)
        {
            var state = task.AsyncState as ValueType;
            if (state is int)
            {
                BlockingCollection<Task> blockingTasks;
                var partitionId = (int)state;
                var threadId = partitionId % _numberOfThreads;
                _tasks.TryGetValue(threadId, out blockingTasks);
                if (blockingTasks != null && !blockingTasks.IsAddingCompleted)
                {
                    blockingTasks.Add(task);
                }
            }
        }

        /// <summary>Provides a list of the scheduled tasks for the debugger to consume.</summary>
        /// <returns>An enumerable of all tasks currently scheduled.</returns>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            // Serialize the contents of the blocking collection of tasks for the debugger
            return _tasks.SelectMany(pair => pair.Value.ToArray());
        }


        /// <summary>Determines whether a Task may be inlined.</summary>
        /// <param name="task">The task to be executed.</param>
        /// <param name="taskWasPreviouslyQueued">Whether the task was previously queued.</param>
        /// <returns>true if the task was successfully inlined; otherwise, false.</returns>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

    }
}