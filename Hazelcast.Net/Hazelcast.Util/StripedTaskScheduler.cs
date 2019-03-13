// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
        private static readonly Random Random = new Random();

        private readonly int _numberOfThreads;
        private readonly List<Thread> _threads;
        private IDictionary<int, BlockingCollection<Task>> _tasks;

        public StripedTaskScheduler(int numberOfThreads, int maximumQueueCapacity=1000000, string threadNamePrefix="hz-striped-scheduler")
        {
            if (numberOfThreads < 1) throw new ArgumentOutOfRangeException("numberOfThreads");

            _numberOfThreads = numberOfThreads;
            // `maximumQueueCapacity` is the given max capacity for this executor. Each worker in this executor should consume
            // only a portion of that capacity. Otherwise we will have `threadCount * maximumQueueCapacity` instead of
            // `maximumQueueCapacity`.
            var perThreadMaxQueueCapacity = (int) Math.Ceiling(1D * maximumQueueCapacity / numberOfThreads);

            _tasks = new Dictionary<int, BlockingCollection<Task>>();
            for (var i = 0; i < numberOfThreads; i++)
            {
                _tasks.Add(i, new BlockingCollection<Task>(perThreadMaxQueueCapacity));
            }

            // Create the threads to be used by this scheduler
            _threads = Enumerable.Range(0, numberOfThreads).Select(i =>
            {
                var thread = new Thread(() => ThreadLoop(i))
                {IsBackground = true, Name = threadNamePrefix +"-" + i};
                return thread;
            }).ToList();

            // Start all of the threads
            _threads.ForEach(t => t.Start());
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
                foreach (var thread in _threads) thread.Join();

                // Cleanup
                foreach (var task in _tasks)
                {
                    task.Value.Dispose();
                }
                _tasks = null;
                _threads.Clear();
            }
        }

        /// <summary>Provides a list of the scheduled tasks for the debugger to consume.</summary>
        /// <returns>An enumerable of all tasks currently scheduled.</returns>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            // Serialize the contents of the blocking collection of tasks for the debugger
            return _tasks.SelectMany(pair => pair.Value.ToArray());
        }

        /// <summary>Queues a Task to be executed by this scheduler.</summary>
        /// <param name="task">The task to be executed.</param>
        protected override void QueueTask(Task task)
        {
            var state = task.AsyncState as ValueType;

            BlockingCollection<Task> blockingTasks;
            int threadId;
            // if task has a partitionId, use that to assign to the correct queue, if not 
            // randomly assign it
            if (state is int)
            {
                var partitionId = Math.Abs((int) state);
                threadId = partitionId%_numberOfThreads;
            }
            else
            {
                threadId = Random.Next(_numberOfThreads);
            }

            _tasks.TryGetValue(threadId, out blockingTasks);
            if (blockingTasks != null && !blockingTasks.IsAddingCompleted)
            {
                blockingTasks.Add(task);
            }
        }


        /// <summary>Determines whether a Task may be inlined.</summary>
        /// <param name="task">The task to be executed.</param>
        /// <param name="taskWasPreviouslyQueued">Whether the task was previously queued.</param>
        /// <returns>true if the task was successfully inlined; otherwise, false.</returns>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        private void ThreadLoop(int threadId)
        {
            BlockingCollection<Task> blockingTasks;
            _tasks.TryGetValue(threadId, out blockingTasks);
            if (blockingTasks == null) return;

            while (!blockingTasks.IsAddingCompleted)
            {
                try
                {
                    var task = blockingTasks.Take();
                    TryExecuteTask(task);
                }
                catch (InvalidOperationException)
                {
                    //BlockingCollection is empty, just ignore it
                }
            }
        }
    }
}