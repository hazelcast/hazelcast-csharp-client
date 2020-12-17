// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Messaging;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Schedules distributed events.
    /// </summary>
    internal class DistributedEventScheduler : IAsyncDisposable
    {
        private readonly Dictionary<int, Queue> _queues = new Dictionary<int, Queue>();
        private readonly object _mutex = new object();
        private readonly ILogger _logger;
        private int _exceptionCount, _unhandledExceptionCount;
        private volatile bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedEventScheduler"/> class.
        /// </summary>
        /// <param name="loggerFactory">A logger factory.</param>
        public DistributedEventScheduler(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger<DistributedEventScheduler>();
        }

        /// <summary>
        /// Triggers when an event handler throws an exception.
        /// </summary>
        public event EventHandler<DistributedEventExceptionEventArgs> OnError;

        /// <summary>
        /// Gets the total count of exceptions thrown by event handlers.
        /// </summary>
        public int ExceptionCount => _exceptionCount;

        /// <summary>
        /// Gets the count of exceptions throw by event handlers, that
        /// were not handled by a <see cref="OnError"/>.
        /// </summary>
        public int UnhandledExceptionCount => _unhandledExceptionCount;

        /// <summary>
        /// (internal for tests only)
        /// Gets the partition tasks count.
        /// </summary>
        internal int Count
        {
            get
            {
                lock (_mutex) return _queues.Count;
            }
        }

        // gets a new queue (could pool)
        private Queue GetQueue() => new Queue();

        // returns an empty queue (could pool)
        private void Return(Queue queue) { }

        // represents a queue and its associated task
        private class Queue
        {
            // might read & write concurrently = concurrent queue
            private readonly ConcurrentQueue<EventData> _items = new ConcurrentQueue<EventData>();

            public void Enqueue(EventData eventData) => _items.Enqueue(eventData);

            public bool TryDequeue(out EventData eventData) => _items.TryDequeue(out eventData);

            public Task Task { get; set; }
        }

        // represents a queued event
        private class EventData
        {
            public int PartitionId { get; set; }

            public ClusterSubscription Subscription { get; set; }

            public ClientMessage Message { get; set; }
        }

        /// <summary>
        /// Adds an event.
        /// </summary>
        /// <param name="subscription">The event subscription.</param>
        /// <param name="eventMessage">The event message.</param>
        /// <returns><c>true</c> if the even has been added successfully; otherwise (if the scheduler
        /// does not accept events anymore, because it has been disposed), <c>false</c>.</returns>
        public bool Add(ClusterSubscription subscription, ClientMessage eventMessage)
        {
            var partitionId = eventMessage.PartitionId;
            var start = false;

            var data = new EventData
            {
                PartitionId = partitionId,
                Subscription = subscription,
                Message = eventMessage
            };

            Queue queue;

            lock (_mutex)
            {
                if (_disposed) return false;

                if (!_queues.TryGetValue(partitionId, out queue))
                {
                    queue = _queues[partitionId] = GetQueue();
                    start = true;
                }

                queue.Enqueue(data);
            }

            if (start)
            {
                queue.Task = Handle(partitionId, queue);
            }
            return true;
        }

        private async Task Handle(int partitionId, Queue queue)
        {
            while (true)
            {
                if (!queue.TryDequeue(out var eventData))
                {
                    lock (_mutex)
                    {
                        if (!queue.TryDequeue(out eventData))
                        {
                            _queues.Remove(partitionId);
                            queue.Task = null;
                            Return(queue);
                            return;
                        }
                    }
                }

                await Handle(eventData); // does not throw
            }
        }

        private async Task Handle(EventData eventData)
        {
            try
            {
                HConsole.WriteLine(this, "Execute event handler");
                await eventData.Subscription.HandleAsync(eventData.Message).CfAwait();
            }
            catch (Exception e)
            {
                HConsole.WriteLine(this, "Handler has thrown.");

                Interlocked.Increment(ref _exceptionCount);
                var args = new DistributedEventExceptionEventArgs(e, eventData.Message);
                var correlationId = eventData.Message.CorrelationId;

                try
                {
                    OnError?.Invoke(this, args);
                }
                catch (Exception ee)
                {
                    var ae = new AggregateException(e, ee);
                    _logger.LogError(ae, $"An event handler [{correlationId}] has thrown an unhandled exception. " +
                                         "In addition, the error handler has also thrown an exception.");
                }

                if (!args.Handled)
                {
                    Interlocked.Increment(ref _unhandledExceptionCount);
                    _logger.LogError(e, $"An event handler [{correlationId}] has thrown an unhandled exception.");
                }
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            Task[] tasks;

            lock (_mutex)
            {
                _disposed = true;
                tasks = _queues.Values.Select(x => x.Task).ToArray();
            }

            await Task.WhenAll(tasks).CfAwait();
        }
    }
}
