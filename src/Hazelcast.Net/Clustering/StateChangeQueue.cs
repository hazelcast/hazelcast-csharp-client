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
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Hazelcast.Core;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Raises client state change events sequentially, in order.
    /// </summary>
    internal class StateChangeQueue : IAsyncDisposable
    {
        private readonly BufferBlock<ClientState> _states = new BufferBlock<ClientState>();
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
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

            _raising = RaiseEvents(_cancel.Token);
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
            if (_states.Post(state)) return;

            // that should not happen, but log to be sure
            _logger.LogWarning($"Failed to add a state {state}.");
        }

        // (background task loop) raises events
        private async Task RaiseEvents(CancellationToken cancellationToken)
        {
            await Task.Yield();

            while (true)
            {
                // fail fast
                cancellationToken.ThrowIfCancellationRequested();

                // wait for a state change
                var state = await _states.ReceiveAsync(cancellationToken).CfAwait();

                // raise event
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

            _cancel.Cancel();
            await _raising.ObserveCanceled().CfAwait();
            _cancel.Dispose();
        }
    }
}
