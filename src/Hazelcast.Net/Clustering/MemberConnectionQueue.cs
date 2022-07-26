﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
    internal class MemberConnectionQueue : IAsyncEnumerable<MemberConnectionRequest>, IAsyncDisposable
    {
        private readonly AsyncQueue<MemberConnectionRequest> _requests = new AsyncQueue<MemberConnectionRequest>();
        private readonly AsyncQueue<MemberConnectionRequest> _delayed = new AsyncQueue<MemberConnectionRequest>();
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

        private readonly SemaphoreSlim _resume = new SemaphoreSlim(0); // blocks the queue when it is suspended
        private readonly SemaphoreSlim _enumerate = new SemaphoreSlim(1); // ensures there can be only 1 concurrent enumerator

        private readonly object _mutex = new object();

        private readonly ILogger _logger;
        private readonly Task _delaying;

        private volatile bool _disposed;
        private MemberConnectionRequest _request;
        private bool _suspended;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberConnectionQueue"/> class.
        /// </summary>
        /// <param name="loggerFactory">A logger factory.</param>
        public MemberConnectionQueue(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<MemberConnectionQueue>();
            _delaying = Delay();

            HConsole.Configure(x => x.Configure<MemberConnectionQueue>().SetPrefix("MBRQ"));
        }

        public event EventHandler<MemberConnectionRequest> ConnectionFailed;

        /// <summary>
        /// (internals for tests only) Gets the count of requests in the queue.
        /// </summary>
        internal int RequestsCount => _requests.Count;

        /// <summary>
        /// (internals for tests only) Gets the count of delayed requests in the queue.
        /// </summary>
        internal int DelayedRequestsCount => _delayed.Count;

        /// <summary>
        /// Suspends the queue.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> that will be completed when the queue is suspended.</returns>
        /// <remarks>
        /// <para>If an item is being processed, this waits for the processing to complete.</para>
        /// <para>When the queue is suspended, calls to the enumerator's MoveNextAsync() method blocks.</para>
        /// </remarks>
        public ValueTask SuspendAsync()
        {
            lock (_mutex)
            {
                if (_disposed) return default; // nothing to suspend - but no need to throw about it

                _logger.IfDebug()?.LogDebug("Suspend the members connection queue.");
                _suspended = true;

                // _request is a struct and cannot be null
                // the default MemberConnectionRequest's Completion is the default ValueTask
                // otherwise, is used to ensure that the request is completed before actually being suspended
                return _request.Completion;
            }
        }

        /// <summary>
        /// Resumes the queue.
        /// </summary>
        /// <remarks>
        /// <para>If <paramref name="drain"/> is <c>true</c>, de-queues and ignores all queued items.</para>
        /// <para>Unblocks calls to the enumerator MoveNextAsync() method.</para>
        /// </remarks>
        public void Resume(bool drain = false)
        {
            lock (_mutex)
            {
                if (_disposed) return; // nothing to resume - but no need to throw about it
                if (!_suspended) throw new InvalidOperationException("Not suspended.");
                _logger.IfDebug()?.LogDebug("{DrainState} the members connection queue.", (drain ? "Drain and resume" : "Resume"));
                if (drain)
                {
                    _requests.ForEach(x => x.Cancel());
                    _delayed.ForEach(x => x.Cancel());
                }
                _suspended = false;
                _resume.Release();
            }
        }

        /// <summary>
        /// Adds a member to connect.
        /// </summary>
        /// <param name="member">The member to connect.</param>
        public void Add(MemberInfo member)
        {
            if (_disposed) return; // no need to add - no need to throw about it

            lock (_mutex)
            {
                if (!_requests.TryWrite(new MemberConnectionRequest(member)))
                {
                    // that should not happen, but log to be sure
                    _logger.IfWarning()?.LogWarning("Failed to add member ({MemberId}).", member.Id.ToShortString());
                }
                else
                {
                    _logger.IfDebug()?.LogDebug("Added member {MemberId}", member.Id.ToShortString());
                }
            }
        }

        /// <summary>
        /// Adds a request again.
        /// </summary>
        /// <param name="request">The request.</param>
        public void AddAgain(MemberConnectionRequest request)
        {
            if (_disposed || request.Cancelled) return; // no need to add - no need to throw about it

            request.Reset();
            _delayed.TryWrite(request);
        }

        private async Task Delay()
        {
            const int minDelay = 1000; // milliseconds - but later on each request could have a retry strategy

            await foreach (var request in _delayed.WithCancellation(_cancel.Token))
            {
                var delay = minDelay - (int)request.Elapsed.TotalMilliseconds;
                _logger.IfDebug()?.LogDebug("Request for member {Member} delayed {Delay}ms", request.Member.Id.ToShortString(), delay > 0 ? delay : 0);
                if (delay > 0) await Task.Delay(delay, _cancel.Token).CfAwait();
                _logger.IfDebug()?.LogDebug("Request for member {Member} queued for retry", request.Member.Id.ToShortString());
                _requests.TryWrite(request);
            }
        }

        // when receiving members from the cluster... if a member is gone,
        // we need to remove it from the queue, no need to ever try to connect
        // to it again - so it remains in the _members async queue, but we
        // flag it so that when we dequeue it, we can ignore it

        /// <summary>
        /// Removes a member.
        /// </summary>
        /// <param name="memberId">The identifier of the member.</param>
        public void Remove(Guid memberId)
        {
            // cancel all corresponding requests - see notes in AsyncQueue, this is best-effort,
            // a member that we want to remove *may* end up being enumerated, and we're going to
            // to to connect to it, and either fail, or drop the connection - accepted tradeoff
            lock (_mutex) {
                _requests.ForEach(x =>
                {
                    if (x.Member.Id == memberId) x.Cancel();
                });
                _delayed.ForEach(x =>
                {
                    if (x.Member.Id == memberId) x.Cancel();
                });
            }
        }

        /// <inheritdoc />
        public IAsyncEnumerator<MemberConnectionRequest> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MemberConnectionQueue));

            // ensure that disposing this class cancels the enumeration
            return new AsyncEnumerator(this, _cancel.Token, cancellationToken);
        }

        private class AsyncEnumerator : IAsyncEnumerator<MemberConnectionRequest>
        {
            private readonly MemberConnectionQueue _queue;
            private readonly CancellationTokenSource _cancellation;
            private IAsyncEnumerator<MemberConnectionRequest> _queueRequestsEnumerator;

            public AsyncEnumerator(MemberConnectionQueue queue, CancellationToken cancellationToken1, CancellationToken cancellationToken2)
            {
                _queue = queue;
                _cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken1, cancellationToken2);
            }

            public async ValueTask<bool> MoveNextAsync()
            {
                if (_cancellation.IsCancellationRequested) return false;

                // if this is the first call, validate that we are only enumerating once at a time & create the enumerator
                if (_queueRequestsEnumerator == null)
                {
                    var acquired = await _queue._enumerate.WaitAsync(TimeSpan.Zero, default).CfAwait();
                    if (!acquired) throw new InvalidOperationException("Can only enumerate once at a time.");
                    _queueRequestsEnumerator = _queue._requests.GetAsyncEnumerator(_cancellation.Token);
                }

                // there is only one consumer, and the consumer *must* complete a request before picking a new one
                if (_queue._request != null && !_queue._request.Completed)
                {
                    throw new InvalidOperationException("Cannot move to next request if previous request has not completed.");
                }

                // loop until we have a valid request to return, because we may dequeue nulls or cancelled members
                while (!_cancellation.IsCancellationRequested)
                {
                    // dequeue a request
                    if (!await _queueRequestsEnumerator.MoveNextAsync().CfAwait())
                        return false;

                    while (!_cancellation.IsCancellationRequested)
                    {
                        // if not suspended, make that request the current one and return - this request is not in the queue
                        // anymore, it's going to be processed no matter what even if the queue is drained or the member is
                        // removed, and then the established connection (if any) will be dropped
                        lock (_queue._mutex)
                        {
                            if (!_queue._suspended)
                            {
                                var request = _queueRequestsEnumerator.Current;
                                if (request.Member == null || request.Cancelled) break; // that request is to be skipped
                                request.Failed += (r, _) => _queue.ConnectionFailed?.Invoke(_queue, (MemberConnectionRequest)r);
                                _queue._request = request;
                                return true;
                            }
                        }

                        // if we reach this point, we did not return nor break = the queue was suspended, wait until it is released
                        // and then loop within the nested while => the dequeued request will be considered again
                        await _queue._resume.WaitAsync(_cancellation.Token).CfAwaitCanceled();
                    }
                }

                return false;
            }

            /// <inheritdoc />
            public MemberConnectionRequest Current => _queue._request;

            public async ValueTask DisposeAsync()
            {
                if (_queueRequestsEnumerator != null)
                {
                    await _queueRequestsEnumerator.DisposeAsync().CfAwait();
                    _queue._enumerate.Release();
                }

                _cancellation.Dispose();
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            // note: DisposeAsync should not throw (CA1065)

            lock (_mutex)
            {
                if (_disposed) return;
                _disposed = true;
            }

            _requests.Complete();
            _delayed.Complete();
            _cancel.Cancel();
            _cancel.Dispose();

            await _delaying.CfAwaitNoThrow();

            // cannot wait until enumeration (if any) is complete,
            // because that depends on the caller calling MoveNext,
            // instead, we return false if the caller calls MoveNext,
            // and the caller should dispose the enumerator

            _resume.Dispose();
            _enumerate.Dispose();
        }
    }
}
