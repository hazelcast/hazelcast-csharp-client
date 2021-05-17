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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Terminates connections.
    /// </summary>
    internal class TerminateConnections : IAsyncDisposable
    {
        private readonly AsyncQueue<MemberConnection> _connections = new AsyncQueue<MemberConnection>();
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

        private readonly Task _terminating;
        private readonly ILogger _logger;

        private volatile int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminateConnections"/> class.
        /// </summary>
        /// <param name="loggerFactory">A logger factory.</param>
        public TerminateConnections(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<TerminateConnections>();

            // start
            _terminating = TerminateAsync(_cancel.Token);
        }

        // throws if this instance has been disposed
        private void ThrowIfDisposed()
        {
            if (_disposed > 0) throw new ObjectDisposedException(nameof(TerminateConnections));
        }

        /// <summary>
        /// Adds a connection to terminate.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public void Add(MemberConnection connection)
        {
            ThrowIfDisposed();

            if (_connections.TryWrite(connection)) return;

            // that should not happen, but log to be sure
            _logger.LogWarning($"Failed to add a connection ({connection}).");
        }

        // (background task loop) terminate connections
        private async Task TerminateAsync(CancellationToken cancellationToken)
        {
            await foreach (var connection in _connections.WithCancellation(cancellationToken))
            {
                // terminate - ok to do it multiple times
                await connection.DisposeAsync().CfAwait();
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            // note: DisposeAsync should not throw (CA1065)

            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            _connections.Complete();
            _cancel.Cancel();
            await _terminating.CfAwaitCanceled();
            _cancel.Dispose();
        }
    }
}
