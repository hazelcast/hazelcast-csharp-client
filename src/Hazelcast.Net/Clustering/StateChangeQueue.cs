// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Raises client state change events sequentially, in order.
    /// </summary>
    internal class StateChangeQueue : IAsyncDisposable
    {
        private readonly AsyncQueue<ClientState> _states = new AsyncQueue<ClientState>();
        private readonly ConcurrentDictionary<int, TaskCompletionSource<object>> _markers = new ConcurrentDictionary<int, TaskCompletionSource<object>>();
        private readonly Task _raising;
        private readonly ILogger _logger;

        private Func<ClientState, ValueTask> _stateChanged;
        private volatile int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateChangeQueue"/> class.
        /// </summary>
        /// <param name="loggerFactory">A logger factory.</param>
        public StateChangeQueue(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<StateChangeQueue>();

            _raising = RaiseEvents();
        }

        // throws if this instance has been disposed
        private void ThrowIfDisposed()
        {
            if (_disposed > 0) throw new ObjectDisposedException(nameof(StateChangeQueue));
        }

        /// <summary>
        /// Occurs when the state has changed.
        /// </summary>
        public Func<ClientState, ValueTask> StateChanged
        {
            get => _stateChanged;
            set
            {
                ThrowIfDisposed();
                _stateChanged = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Adds a state change to the queue.
        /// </summary>
        /// <param name="state">The new state.</param>
        public void Add(ClientState state)
        {
            ThrowIfDisposed();
            if (_states.TryWrite(state)) return;

            FailAdd(state);
        }

        /// <summary>
        /// Adds a state change to the queue and wait for the corresponding event.
        /// </summary>
        /// <param name="state">The new state.</param>
        /// <returns>A task that will complete when the event corresponding to the state
        /// has been fully handled.</returns>
        public async Task AddAndWait(ClientState state)
        {
            ThrowIfDisposed();

            int marker;

            // get a marker
            var completion = new TaskCompletionSource<object>();
            do
            {
                marker = -RandomProvider.Next();
            } while (marker < 0 && !_markers.TryAdd(marker, completion));

            // queue the state
            if (!_states.TryWrite(state))
            {
                // embarrassing - should never happen, our code should *not*
                // dispose the state change queue while adding states!
                FailAdd(state);
                return;
            }

            // queue the marker
            if (!_states.TryWrite((ClientState)marker))
            {
                // embarrassing - should never happen, our code should *not*
                // dispose the state change queue while adding states!
                FailAdd(state);
                return;
            }

            // wait until the marker is hit
            await completion.Task.CfAwait();
        }


        // fails to add a state
        private void FailAdd(ClientState state)
        {
            _logger.IfWarning()?.LogWarning("Failed to add a state {State}.", state);
            ThrowIfDisposed();
        }

        // (background task loop) raises events
        private async Task RaiseEvents()
        {
            await foreach (var state in _states)
            {
                var marker = (int)state;
                if (marker < 0)
                {
                    if (_markers.TryRemove(marker, out var completion))
                        completion.TrySetResult(null);
                    continue;
                }

                try
                {
                    await _stateChanged.AwaitEach(state).CfAwait();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Caught error while raising StateChanged ({state}).");
                }
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            // note: DisposeAsync should not throw (CA1065)

            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            // stop accepting new states
            _states.Complete();

            // wait until the events queue is drained
            await _raising.CfAwait();
        }
    }
}
