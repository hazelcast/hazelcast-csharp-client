// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Models;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering;

internal class MemberConnectionQueue : IAsyncEnumerable<MemberConnectionRequest>, IAsyncDisposable
{
    public const int TimeMargin = 10; // ms

    private readonly object _mutex = new();
    private readonly LinkedList<MemberConnectionRequest> _requests = new();
    private readonly Func<Guid, bool> _isMember;
    private readonly Func<Guid, bool> _shouldConnect;
    private readonly ILogger _logger;

    private bool _disposed;
    private CancellationTokenSource? _enumeratorCancellation;
    private TaskCompletionSource<object?> _free;
    private TaskCompletionSource<object?> _available;
    private TaskCompletionSource<object?> _enabled;

    public MemberConnectionQueue(Func<Guid, bool> isMember, Func<Guid, bool> shouldConnect, ILoggerFactory loggerFactory)
    {
        _isMember = isMember;
        _shouldConnect = shouldConnect;
        _logger = loggerFactory.CreateLogger<MemberConnectionQueue>();

        _free = NewTaskCompletionSource(); // completed when the queue is free (no pending request)
        _available = NewTaskCompletionSource(); // completed when requests are available
        _enabled = NewTaskCompletionSource(); // completed when the queue is enabled (not suspended)

        _free.TrySetResult(null); // nothing is pending
        //_available is not completed, nothing is available
        //_enabled is not completed, queue is initially suspended
    }

    private static TaskCompletionSource<object?> NewTaskCompletionSource()
        => new(TaskCreationOptions.RunContinuationsAsynchronously);

    private bool TryTakeFirst([NotNullWhen(true)] out MemberConnectionRequest? request)
    {
        // this is always invoked from within a _mutex lock

        var n = _requests.First;

        if (n != null)
        {
            var delay = (int) (n.Value.Time - Clock.Milliseconds - TimeMargin);
            if (delay <= 0)
            {
                _requests.RemoveFirst();
                request = n.Value;
                _logger.IfDebug()?.LogDebug($"Take request member={request.Member.Id.ToShortString()}");
                return true;
            }

            // queue contains an item that will become available after a delay
            _available = NewTaskCompletionSource(); // can't be waiting
            _ = Delay(delay, _available);
        }
        else
        {
            // queue is empty
            _available = NewTaskCompletionSource(); // can't be waiting
        }

        request = null;
        return false;
    }

    private async Task Delay(int delay, TaskCompletionSource<object?> available)
    {
        await Task.Delay(delay).CfAwait();

        lock (_mutex)
        {
            if (available == _available) _available.TrySetResult(null);
        }
    }

    public int Count
    {
        get
        {
            lock (_mutex) return _requests.Count;
        }
    }

    public void Add(MemberInfo member)
    {
        if (_shouldConnect?.Invoke(member.Id) == false)
        {
            _logger.IfDebug()?.LogDebug($"Connection request rejected for member={member.Id.ToShortString()} because it is not in the filtered member list.");
            return;
        }

        var request = new MemberConnectionRequest(member);

        request.Completed += (_, ea) =>
        {
            _logger.IfDebug()?.LogDebug($"Request completed member={request.Member.Id.ToShortString()} result={(ea.Success ? "success" : "failed")}");

            lock (_mutex) _free.TrySetResult(null);
            if (ea.Success || !_isMember(member.Id)) return;

            request.Time = Clock.Milliseconds + 1000;
            Add(request);
        };

        Add(request);
    }

    private void Add(MemberConnectionRequest request)
    {
        _logger.IfDebug()?.LogDebug($"Add request member={request.Member.Id.ToShortString()} time={(request.Time - Clock.Milliseconds)}ms");

        lock (_mutex)
        {
            var n = _requests.First;
            if (n == null || n.Value.Time > request.Time)
            {
                _requests.AddFirst(request);
            }
            else
            {
                while (n != null && n.Value.Time <= request.Time) n = n.Next;
                if (n == null) _requests.AddLast(request);
                else _requests.AddBefore(n, request);
            }

            _available.TrySetResult(null);
        }
    }

    public void Remove(Guid memberId)
    {
        lock (_mutex)
        {
            var n = _requests.First;
            while (n != null && n.Value.Member.Id != memberId) n = n.Next;
            if (n != null)
            {
                _logger.IfDebug()?.LogDebug($"Remove request member={n.Value.Member.Id.ToShortString()}");
                _requests.Remove(n);
            }
        }
    }

    public ValueTask SuspendAsync()
    {
        Task task;
        lock (_mutex)
        {
            if (!_enabled.Task.IsCompleted) return default;
            _logger.IfDebug()?.LogDebug("Suspend");
            _enabled.TrySetResult(null); // in case we are waiting
            _enabled = NewTaskCompletionSource();
            task = _free.Task;
        }

        return new ValueTask(task);
    }

    public void Clear()
    {
        lock (_mutex)
        {
            _requests.Clear();
            _logger.IfDebug()?.LogDebug("Clear");
            _available.TrySetResult(null); // in case we are waiting
            _available = NewTaskCompletionSource();
        }
    }

    public void Resume(bool clear = false)
    {
        lock (_mutex)
        {
            if (_enabled.Task.IsCompleted) return;
            _logger.IfDebug()?.LogDebug($"Resume{(clear ? " (clear)" : "")}");
            if (clear)
            {
                _requests.Clear();
                _available.TrySetResult(null); // in case we are waiting
                _available = NewTaskCompletionSource();
            }
            _enabled.TrySetResult(null);
        }
    }

    public ValueTask<MemberConnectionRequest?> TakeAsync(CancellationToken cancellationToken)
    {
        lock (_mutex)
        {
            if (_disposed || cancellationToken.IsCancellationRequested)
                return new ValueTask<MemberConnectionRequest?>((MemberConnectionRequest?) null);

            if (!_free.Task.IsCompleted)
                throw new InvalidOperationException("A request is already in progress.");

            if (_enabled.Task.IsCompleted && TryTakeFirst(out var request))
            {
                _free = NewTaskCompletionSource();
                return new ValueTask<MemberConnectionRequest?>(request);
            }
        }

        return WaitAndTakeAsync(cancellationToken);
    }

    private async ValueTask<MemberConnectionRequest?> WaitAndTakeAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                // _enabled or _available may complete because they are being replaced,
                // so the queue is not really enabled nor available, that will be taken
                // care of in the locked block below and just spin this while loop once.
                await Task.WhenAll(_enabled.Task, _available.Task).WaitAsync(cancellationToken).CfAwait();
            }
            catch (OperationCanceledException)
            {
                return null;
            }

            lock (_mutex)
            {
                if (_disposed || cancellationToken.IsCancellationRequested)
                    return null;

                if (_enabled.Task.IsCompleted && TryTakeFirst(out var request))
                {
                    _free = NewTaskCompletionSource();
                    return request;
                }
            }
        }
    }

    public IAsyncEnumerator<MemberConnectionRequest> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        lock (_mutex)
        {
            if (_enumeratorCancellation != null)
                throw new InvalidOperationException("Queue is already enumerating.");
            _enumeratorCancellation = new CancellationTokenSource().LinkedWith(cancellationToken);
            return new AsyncEnumerator(this, _enumeratorCancellation.Token);
        }
    }

    private void ReturnAsyncEnumerator()
    {
        lock (_mutex)
        {
            _enumeratorCancellation!.Dispose();
            _enumeratorCancellation = null;
        }
    }

    private class AsyncEnumerator : IAsyncEnumerator<MemberConnectionRequest>
    {
        private readonly MemberConnectionQueue _queue;
        private readonly CancellationToken _cancellationToken;

        public AsyncEnumerator(MemberConnectionQueue queue, CancellationToken cancellationToken)
        {
            _queue = queue;
            _cancellationToken = cancellationToken;
            Current = null!;
        }

        public MemberConnectionRequest Current { get; private set; }

        public async ValueTask<bool> MoveNextAsync()
        {
            var current = await _queue.TakeAsync(_cancellationToken).CfAwait();
            if (current != null) Current = current;
            return current != null;
        }

        public ValueTask DisposeAsync()
        {
            _queue.ReturnAsyncEnumerator();
            return default;
        }
    }

    public ValueTask DisposeAsync()
    {
        lock (_mutex)
        {
            if (_disposed) return default;
            _disposed = true;
        }

        _enumeratorCancellation?.Cancel();

        return default;
    }
}
