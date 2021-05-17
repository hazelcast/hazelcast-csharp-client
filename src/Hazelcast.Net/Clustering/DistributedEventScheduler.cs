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
    /// <remarks>
    /// <para>Events related to a partition id (where eventMessage.PartitionId > 0) run sequentially
    /// on a queue specific to that partition. Other events (where eventMessage.PartitionId == 0) all
    /// run sequentially on their own queue.</para>
    /// <para>Handlers exceptions are caught and logged, unless captured via the <see cref="HandlerError"/>
    /// event and marked as handled. In any case, handlers exceptions cannot break the scheduler.</para>
    /// </remarks>
    internal class DistributedEventScheduler : IAsyncDisposable
    {
        private readonly SimpleObjectPool<Queue> _pool;
        private readonly Dictionary<int, Queue> _queues = new Dictionary<int, Queue>();
        private readonly object _mutex = new object();
        private readonly ILogger _logger;
        private int _exceptionCount, _unhandledExceptionCount;
        private volatile bool _disposed;

        static DistributedEventScheduler()
        {
            HConsole.Configure(x => x.Configure<DistributedEventScheduler>().SetPrefix("EVTS.SCHED"));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedEventScheduler"/> class.
        /// </summary>
        /// <param name="loggerFactory">A logger factory.</param>
        public DistributedEventScheduler(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger<DistributedEventScheduler>();

            // TODO: how many queues should we retain?
            const int size = 10;
            _pool = new SimpleObjectPool<Queue>(() => new Queue(), size);

            HConsole.Configure(x => x.Configure<DistributedEventScheduler>().SetPrefix("EVENTS"));
        }

        /// <summary>
        /// Triggers when an event handler throws an exception.
        /// </summary>
        public event EventHandler<DistributedEventExceptionEventArgs> HandlerError;

        /// <summary>
        /// Gets the total count of exceptions thrown by event handlers.
        /// </summary>
        public int ExceptionCount => _exceptionCount;

        /// <summary>
        /// Gets the count of exceptions throw by event handlers, that
        /// were not handled by a <see cref="HandlerError"/>.
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
                if (_disposed)
                {
                    HConsole.WriteLine(this, $"Discard event, correlation:{eventMessage.CorrelationId}");
                    return false;
                }

                HConsole.WriteLine(this, $"Enqueue event, correlation:{eventMessage.CorrelationId} queue:{partitionId}");

                if (!_queues.TryGetValue(partitionId, out queue))
                {
                    HConsole.WriteLine(this, $"Create queue:{partitionId}");
                    queue = _queues[partitionId] = _pool.Get();
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
            // using (!_disposed) condition here instead of (true) means
            // that on shutdown queued events will be dropped - otherwise,
            // we could have a ton of events to process and shutting down
            // would take time - TODO: is this the right decision?

            while (!_disposed)
            {
                if (!queue.TryDequeue(out var eventData))
                {
                    lock (_mutex)
                    {
                        if (!queue.TryDequeue(out eventData))
                        {
                            HConsole.WriteLine(this, $"Release queue:{partitionId}");
                            _queues.Remove(partitionId);
                            queue.Task = null;
                            _pool.Return(queue);
                            return;
                        }
                    }
                }

                await Handle(eventData).CfAwait(); // does not throw
            }
        }

        private async Task Handle(EventData eventData)
        {
            try
            {
                HConsole.WriteLine(this, $"Handle event, correlation:{eventData.Message.CorrelationId} queue:{eventData.PartitionId}");
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
                    HandlerError?.Invoke(this, args);
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
                tasks = _queues.Values.Select(x => x.Task).Where(x => x != null).ToArray();
            }

            await Task.WhenAll(tasks).CfAwait();
        }
    }
}
