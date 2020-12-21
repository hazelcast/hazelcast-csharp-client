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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Models;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents the queue of members that need to be connected.
    /// </summary>
    internal class MemberConnectionQueue : IAsyncEnumerable<(MemberInfo, CancellationToken)>, IAsyncDisposable
    {
        private readonly AsyncQueue<MemberInfo> _members = new AsyncQueue<MemberInfo>();
        private readonly Dictionary<Guid, bool> _removed = new Dictionary<Guid, bool>();
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly SemaphoreSlim _resume = new SemaphoreSlim(0);
        private readonly SemaphoreSlim _enumerate = new SemaphoreSlim(1);
        private readonly object _mutex = new object();

        private readonly ILogger _logger;

        private readonly CancellationTokenRegistration _reg;
        private volatile bool _disposed;
        private CancellationTokenSource _itemCancel;
        private int _suspend;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberConnectionQueue"/> class.
        /// </summary>
        /// <param name="loggerFactory">A logger factory.</param>
        public MemberConnectionQueue(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<MemberConnectionQueue>();

            HConsole.Configure(consoleOptions => consoleOptions.Set(this, x => x.SetPrefix("MBRQ")));
        }

        /// <summary>
        /// Suspends the queue.
        /// </summary>
        public void Suspend()
        {
            lock (_mutex)
            {
                if (_disposed) return; // nothing to suspend - but no need to throw about it

                // cancel any current item
                if (++_suspend == 1) _itemCancel?.Cancel();
            }
        }

        /// <summary>
        /// Resumes the queue.
        /// </summary>
        public void Resume(bool drain = false)
        {
            lock (_mutex)
            {
                if (_disposed) return; // nothing to resume - but no need to throw about it

                if (_suspend == 0) throw new InvalidOperationException("Not suspended.");

                if (drain) _members.Drain();

                if (--_suspend == 0) _resume.Release();
            }
        }

        /// <summary>
        /// Adds a member to connect.
        /// </summary>
        /// <param name="member">The member to connect.</param>
        public void Add(MemberInfo member)
        {
            if (_disposed) return; // no need to add - no need to throw about it

            if (_members.TryWrite(member)) return;

            lock (_mutex) _removed[member.Id] = false;

            // that should not happen, but log to be sure
            _logger.LogWarning($"Failed to add a member ({member}).");
        }

        public void Remove(Guid memberId)
        {
            lock (_mutex) if (_removed.ContainsKey(memberId)) _removed[memberId] = true;
        }

        /// <inheritdoc />
        public IAsyncEnumerator<(MemberInfo, CancellationToken)> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MemberConnectionQueue));

            // ensure that disposing this class cancels the enumeration
            return new AsyncEnumerator(this, _cancel.Token, cancellationToken);
        }

        private class AsyncEnumerator : IAsyncEnumerator<(MemberInfo, CancellationToken)>
        {
            private readonly MemberConnectionQueue _queue;
            private readonly CancellationTokenSource _cancellation;
            private IAsyncEnumerator<MemberInfo> _queueEnumerator;

            public AsyncEnumerator(MemberConnectionQueue queue, CancellationToken cancellationToken1, CancellationToken cancellationToken2)
            {
                _queue = queue;
                _cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken1, cancellationToken2);
            }

            public async ValueTask<bool> MoveNextAsync()
            {
                if (_cancellation.IsCancellationRequested) return false;

                // only one enumerator at a time
                if (_queueEnumerator == null)
                {
                    var acquired = await _queue._enumerate.WaitAsync(TimeSpan.Zero, default).CfAwait();
                    if (!acquired) throw new InvalidOperationException("Can only enumerate once at a time.");
                    _queueEnumerator = _queue._members.GetAsyncEnumerator(_cancellation.Token);
                }

                // when suspending:
                // - current item, if any, is canceled
                // - everything else is blocked until resumed

                lock (_queue._mutex)
                {
                    if (_queue._suspend == 0)
                    {
                        _queue._itemCancel?.Dispose();
                        _queue._itemCancel = null;
                    }
                }

                while (!_cancellation.IsCancellationRequested)
                {
                    // this is blocking and returns true once a member is available,
                    // or false if the queue enumerator is complete (no more members)
                    if (!await _queueEnumerator.MoveNextAsync())
                        return false;

                    var member = _queueEnumerator.Current;

                    // skip nulls, go wait for another member
                    if (member == null)
                        continue;

                    // we have a candidate member
                    while (!_cancellation.IsCancellationRequested)
                    {
                        // if not suspended, return, else wait and try again
                        lock (_queue._mutex)
                        {
                            if (_queue._suspend == 0)
                            {
                                if (_queue._removed.TryGetValue(member.Id, out var removed))
                                {
                                    _queue._removed.Remove(member.Id);
                                    if (removed) break;
                                }

                                HConsole.WriteLine(this, $"Member {member.Uuid.ToShortString()}: connect");
                                _queue._itemCancel = new CancellationTokenSource();
                                return true;
                            }
                        }

                        await _queue._resume.WaitAsync(_cancellation.Token).CfAwaitCanceled();
                    }

                    if (!_cancellation.IsCancellationRequested)
                        HConsole.WriteLine(this, $"Member {member.Uuid.ToShortString()}: skip");
                }

                return false;
            }

            /// <inheritdoc />
            public (MemberInfo, CancellationToken) Current => (_queueEnumerator.Current, _queue._itemCancel.Token);

            public async ValueTask DisposeAsync()
            {
                if (_queueEnumerator != null)
                {
                    await _queueEnumerator.DisposeAsync().CfAwait();
                    _queue._enumerate.Release();
                }

                _cancellation.Dispose();
            }
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            // note: DisposeAsync should not throw (CA1065)

            lock (_mutex)
            {
                if (_disposed) return default;
                _disposed = true;
            }

            _members.Complete();
            _cancel.Cancel();
            _cancel.Dispose();

            // cannot wait until enumeration (if any) is complete,
            // because that depends on the caller calling MoveNext,
            // instead, we return false if the caller calls MoveNext,
            // and the caller should dispose the enumerator

            _resume.Dispose();

            return default;
        }
    }
}
