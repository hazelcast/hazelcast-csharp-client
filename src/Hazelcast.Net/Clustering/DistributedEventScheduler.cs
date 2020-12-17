// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
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
        private readonly Dictionary<int, Task> _partitionTasks = new Dictionary<int, Task>();
        private readonly Func<Task, object, Task> _continueWithHandler;
        private readonly Action<Task, object> _removeAfterUse;
        private readonly object _mutex = new object();
        private readonly ILogger _logger;
        private bool _disposed;
        private int _exceptionCount, _unhandledExceptionCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedEventScheduler"/> class.
        /// </summary>
        /// <param name="loggerFactory">A logger factory.</param>
        public DistributedEventScheduler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory?.CreateLogger<DistributedEventScheduler>() ?? throw new ArgumentNullException(nameof(loggerFactory));

            _continueWithHandler = ContinueWithHandler;
            _removeAfterUse = RemoveAfterUse;
        }

        /// <summary>
        /// (internal for tests only)
        /// Gets the partition tasks count.
        /// </summary>
        internal int PartitionTasksCount => _partitionTasks.Count;

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
        /// Adds an event.
        /// </summary>
        /// <param name="subscription">The event subscription.</param>
        /// <param name="eventMessage">The event message.</param>
        /// <returns><c>true</c> if the even has been added successfully; otherwise (if the scheduler
        /// does not accept events anymore, because it has been disposed), <c>false</c>.</returns>
        public bool Add(ClusterSubscription subscription, ClientMessage eventMessage)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (eventMessage == null) throw new ArgumentNullException(nameof(eventMessage));

            var partitionId = eventMessage.PartitionId;
            var state = new State { Subscription = subscription, Message = eventMessage, PartitionId = partitionId };

            // the factories in ConcurrentDictionary.AddOrUpdate are *not* thread-safe, i.e. in order
            // to run with minimal locking, the ConcurrentDictionary may run the two of them, or one
            // of them multiple times, and only guarantees that one single unique value ends up in the
            // dictionary - in our case, that would be a problem, since the factories spawn tasks.
            //
            // a traditional way around this consists in having the factories return Lazy<Task> so
            // that AddOrUpdate returns a Lazy<Task> and one single unique task is created when getting
            // the .Value of that lazy. however this (a) adds another layer of locking, (b) implies
            // captures since Lazy<T> does not have a constructor that accept factory arguments, etc.
            //
            // this is annoying - so we are going with a normal dictionary and a global lock for now.
            //
            // ideas:
            // avoid creating a Lazy per task, but manage a per-partition lock (so we only lock the
            // partition, not the whole dictionary) - yet that would mean a concurrent dictionary of
            // locks, etc?

            /*
            _lock.EnterReadLock();
            try
            {
                if (_disposed) return false;

                _ = _partitionTasks
                    .AddOrUpdate(partitionId, CreateFirstTask, AppendNextTask, state)
                    .ContinueWith(_clearAfterUse, state, default, TaskContinuationOptions.None, TaskScheduler.Current);

                return true;
            }
            finally
            {
                _lock.ExitReadLock();
            }
            */

            Task task;

            lock (_mutex)
            {
                if (_disposed) return false;

                if (!_partitionTasks.TryGetValue(partitionId, out task))
                    task = Task.CompletedTask;

                task = AddContinuation(task, state);
                _partitionTasks[partitionId] = task;
            }

            task.ContinueWith(_removeAfterUse, state, default, TaskContinuationOptions.None, TaskScheduler.Current);
            return true;
        }

        private class State // captures things once
        {
            public ClusterSubscription Subscription { get; set; }

            public ClientMessage Message { get; set; }

            public int PartitionId { get; set; }
        }

        private Task AddContinuation(Task task, State state)
            => task.ContinueWith(_continueWithHandler, state, default, TaskContinuationOptions.None, TaskScheduler.Current).Unwrap();

        /*
        private Task CreateFirstTask(int _, State state)
            => AddContinuation(Task.CompletedTask, state);

        private Task AppendNextTask(int _, Task task, State state)
            => AddContinuation(task, state);
        */

        private void RemoveAfterUse(Task task, object stateObject)
        {
            var state = (State) stateObject;

            lock (_mutex) _partitionTasks.TryRemove(state.PartitionId, task);
        }

        private async Task ContinueWithHandler(Task task, object stateObject)
        {
            var state = (State) stateObject;

            try
            {
                HConsole.WriteLine(this, "Execute event handler");
                await state.Subscription.HandleAsync(state.Message).CfAwait();
            }
            catch (Exception e)
            {
                HConsole.WriteLine(this, "Handler has thrown.");

                Interlocked.Increment(ref _exceptionCount);
                var args = new DistributedEventExceptionEventArgs(e, state.Message);
                var correlationId = state.Message.CorrelationId;

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
            lock (_mutex) _disposed = true;

            /*
            _lock.EnterWriteLock();
            try
            {
                _disposed = true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            */

            try
            {
                // ReSharper disable once InconsistentlySynchronizedField - _disposed is true, all is safe
                await Task.WhenAll(_partitionTasks.Values).CfAwait();
            }
            catch (Exception e)
            {
                // this should never happen, but better be safe
                _logger.LogError(e, "Caught an exception while disposing.");
            }

            /*
            _lock.Dispose();
            */
        }
    }
}
