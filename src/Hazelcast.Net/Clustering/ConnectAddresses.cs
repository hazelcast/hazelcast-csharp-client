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
using System.Threading.Tasks.Dataflow;
using Hazelcast.Core;
using Hazelcast.Networking;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Connects addresses.
    /// </summary>
    internal class ConnectAddresses : IAsyncDisposable
    {
        private readonly BufferBlock<NetworkAddress> _addresses = new BufferBlock<NetworkAddress>();
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly SemaphoreSlim _drained = new SemaphoreSlim(0);
        private readonly SemaphoreSlim _paused = new SemaphoreSlim(0);
        private readonly SemaphoreSlim _resume = new SemaphoreSlim(0);

        private readonly Func<NetworkAddress, CancellationToken, Task> _connect;
        private readonly Task _connecting;
        private readonly ILogger _logger;

        private volatile int _disposed;
        private volatile bool _draining;
        private volatile bool _pausing;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectAddresses"/> class.
        /// </summary>
        /// <param name="connect">The connect function.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public ConnectAddresses(Func<NetworkAddress, CancellationToken, Task> connect, ILoggerFactory loggerFactory)
        {
            _connect = connect ?? throw new ArgumentNullException(nameof(connect));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<ConnectAddresses>();

            // start
            _connecting = ConnectAsync(_cancel.Token);
        }

        // throws if this instance has been disposed
        private void ThrowIfDisposed()
        {
            if (_disposed > 0) throw new ObjectDisposedException(nameof(ConnectAddresses));
        }

        /// <summary>
        /// Waits for a pending connection, if any, then pauses the task.
        /// </summary>
        /// <returns></returns>
        public async ValueTask PauseAsync()
        {
            ThrowIfDisposed();

            if (_pausing) throw new InvalidOperationException("Already paused.");

            _pausing = true;
            _addresses.Post(null); // force receive
            await _paused.WaitAsync().CAF();
        }

        /// <summary>
        /// Resumes the task.
        /// </summary>
        /// <param name="drain">Whether to drain the queue before resuming.</param>
        /// <returns>A task that will complete when the task has resumed.</returns>
        public async ValueTask ResumeAsync(bool drain = false)
        {
            ThrowIfDisposed();

            if (!_pausing) throw new InvalidOperationException("Not paused");

            if (drain)
            {
                _draining = true;
                _addresses.Post(null); // force receive
            }

            _resume.Release();

            if (drain)
            {
                await _drained.WaitAsync().CAF();
            }
        }

        /// <summary>
        /// Adds an address to connect.
        /// </summary>
        /// <param name="address">The address to connect.</param>
        public void Add(NetworkAddress address)
        {
            ThrowIfDisposed();

            if (_addresses.Post(address)) return;

            // that should not happen, but log to be sure
            _logger.LogWarning($"Failed to add an address ({address}).");
        }

        // (background task loop) connect addresses
        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            // TODO: consider throttling?

            await Task.Yield();

            while (true)
            {
                // fail fast
                cancellationToken.ThrowIfCancellationRequested();

                // wait for an address, or the special Drainer or Ignored members
                var address = await _addresses.ReceiveAsync(cancellationToken).CAF();

                // pause
                if (_pausing)
                {
                    _paused.Release();
                    await _resume.WaitAsync(cancellationToken).CAF();
                    _pausing = false;
                }

                // drain
                if (_draining)
                {
                    if (_addresses.Count == 0) // drained
                    {
                        _draining = false;
                        _drained.Release();
                    }
                    // else just ignore the member (draining the queue)
                }

                // connect, if not ignored
                else if (!(address is null))
                {
                    try
                    {
                        await _connect(address, cancellationToken).CAF();
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Caught exception while trying to connect an address.");
                        _addresses.Post(address); // try again
                    }
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
            await _connecting.ObserveCanceled().CAF();
            _cancel.Dispose();

            _paused.Dispose();
            _drained.Dispose();
            _resume.Dispose();
        }
    }
}