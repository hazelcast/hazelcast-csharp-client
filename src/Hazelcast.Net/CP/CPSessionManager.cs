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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Microsoft.Extensions.Logging;

namespace Hazelcast.CP;

/// <summary>
/// Manages server side CP session requests and heartbeat.
/// </summary>
internal partial class CPSessionManager : IAsyncDisposable
{
    /// <summary>
    /// Gets the identifier representing the absence of a session.
    /// </summary>
    public const long NoSessionId = -1;

    private readonly ConcurrentDictionary<CPGroupId, SemaphoreSlim> _groupSemaphores = new();
    private readonly ConcurrentDictionary<CPGroupId, CPSession> _groupSessions = new();
    private readonly ConcurrentDictionary<(CPGroupId, long), long> _uniqueThreadIds = new();
    private readonly AsyncReaderWriterLock _lock = new();
    private readonly ILogger _logger;
    private readonly Cluster _cluster;

    private bool _running = true;
    private int _disposed;

    private readonly CancellationTokenSource _heartbeatCancel = new();
    private Task _heartbeatTask = Task.CompletedTask;
    private int _heartbeatRunning;

    /// <summary>
    /// Initializes a new instance of the <see cref="CPSessionManager"/> class.
    /// </summary>
    /// <param name="cluster">The cluster object.</param>
    public CPSessionManager(Cluster cluster)
    {
        _cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
        _logger = _cluster.State.LoggerFactory.CreateLogger<CPSessionManager>();
        HConsole.Configure(x => x.Configure<Heartbeat>().SetPrefix("CP.SESSION"));
    }

    private void ThrowIfDisposed()
    {
        if (_disposed == 1) throw new ObjectDisposedException(nameof(CPSessionManager));
    }

    private void ThrowIfNotRunning()
    {
        if (!_running) throw new InvalidOperationException("The session manager has been shut down.");
    }

    #region Threads

    public ValueTask<long> GetOrCreateUniqueThreadIdAsync(CPGroupId groupId, long localThreadId)
    {
        ThrowIfDisposed();

        // Java *always* locks - which means we cannot optimize a non-async path?
        //await _lock.ReadLockAsync().CfAwait();
        //
        // yet, it's equivalent to first synchronously look for an id in the
        // dictionary, *then* fall back to asynchronous code which locks

        var key = (groupId, localThreadId);
        return _uniqueThreadIds.TryGetValue(key, out var id)
            ? new ValueTask<long>(id)
            : CreateUniqueThreadIdAsync(key);

        async ValueTask<long> CreateUniqueThreadIdAsync((CPGroupId GroupId, long LockId) k)
        {
            using var _ = await _lock.ReadLockAsync().CfAwait();
            ThrowIfDisposed();
            return _uniqueThreadIds.GetOrAdd(k, await RequestGenerateThreadIdAsync(k.GroupId).CfAwait());
        }
    }

    #endregion

    #region SessionManagement

    /// <summary>
    /// Acquires the CP session for the specified CP group.
    /// </summary>
    /// <param name="groupId">The CP group identifier.</param>
    /// <param name="count">The number of acquisitions.</param>
    /// <returns>The CP session identifier.</returns>
    public async Task<long> AcquireSessionAsync(CPGroupId groupId, int count = 1)
    {
        ThrowIfDisposed();
        var session = await GetOrCreateSessionAsync(groupId).CfAwait();
        session.Acquire(count);
        return session.Id;
    }

    /// <summary>
    /// Releases the specified CP session.
    /// </summary>
    /// <param name="groupId">The CP group identifier.</param>
    /// <param name="sessionId">The CP session identifier.</param>
    /// <param name="count">The number of releases.</param>
    public void ReleaseSession(CPGroupId groupId, long sessionId, int count = 1)
    {
        ThrowIfDisposed();
        if (_groupSessions.TryGetValue(groupId, out var sessionState) && sessionState.Id == sessionId)
        {
            sessionState.Release(count);
        }
    }

    /// <summary>
    /// Invalidates the specified CP session.
    /// </summary>
    /// <remarks>
    /// <para>Once a CP session has been invalidated, heartbeat stops.</para>
    /// </remarks>
    /// <param name="groupId">The CP group identifier.</param>
    /// <param name="sessionId">The CP session identifier.</param>
    public void InvalidateSession(CPGroupId groupId, long sessionId)
    {
        ThrowIfDisposed();
        if (_groupSessions.TryGetValue(groupId, out var sessionState) && sessionState.Id == sessionId)
        {
            _groupSessions.TryRemove(groupId, sessionState);
        }
    }

    /// <summary>
    /// Gets the CP session identifier corresponding to the specified CP group identifier, if any.
    /// </summary>
    /// <param name="groupId">The CP group identifier.</param>
    /// <returns>The CP session identifier, if any; otherwise <see cref="NoSessionId"/>.</returns>
    public long GetSessionId(CPGroupId groupId)
    {
        ThrowIfDisposed();
        return _groupSessions.TryGetValue(groupId, out var sessionState)
            ? sessionState.Id
            : NoSessionId;
    }

    /// <summary>
    /// Closes a CP session.
    /// </summary>
    /// <param name="groupId">The CP group identifier.</param>
    /// <param name="sessionId">The CP session identifier.</param>
    /// <returns></returns>
    public async Task CloseSessionAsync(CPGroupId groupId, long sessionId)
    {
        ThrowIfDisposed();
        InvalidateSession(groupId, sessionId);
        await RequestCloseSessionAsync(groupId, sessionId).CfAwait();
    }

    /// <summary>
    /// Invokes a shutdown call on server to close all existing sessions.
    /// </summary>
    /// <remarks>
    /// <para>This method stops the session manager, and any attempt to further
    /// obtain a session will cause an exception to be throws. Nevertheless,
    /// the session manager still needs to be properly disposed.</para>
    /// </remarks>
    public async Task ShutdownAsync()
    {
        using var _ = await _lock.WriteLockAsync().CfAwait();

        _running = false;
        await _groupSessions.ParallelForEachAsync((entry, _) =>
        {
            var (groupId, sessionId) = entry;
            return CloseSessionAsync(groupId, sessionId.Id);
        }).CfAwait();

        _groupSessions.Clear();
    }

    /// <summary>
    /// Gets or creates a CP session for a CP group.
    /// </summary>
    /// <param name="groupId">The CP group identifier.</param>
    /// <returns>The <see cref="CPSession"/> for the CP group.</returns>
    private async Task<CPSession> GetOrCreateSessionAsync(CPGroupId groupId)
    {
        using var _ = await _lock.ReadLockAsync().CfAwait();

        ThrowIfDisposed();
        ThrowIfNotRunning();

        // double-check
        if (_groupSessions.TryGetValue(groupId, out var sessionState) && sessionState.IsValid)
            return sessionState;

        // acquire lock for this group only
        var groupSemaphore = GetGroupSemaphore(groupId);
        await groupSemaphore.WaitAsync().CfAwait();

        try
        {
            // triple-check
            if (_groupSessions.TryGetValue(groupId, out sessionState) && sessionState.IsValid)
                return sessionState;

            // actually create/start a new session
            var (session, heartbeatMillis) = await RequestNewSessionAsync(groupId).CfAwait();
            _groupSessions[groupId] = session;
            EnsureHeartbeat(TimeSpan.FromMilliseconds(heartbeatMillis));
            return session;
        }
        finally
        {
            groupSemaphore.Release();
        }
    }

    /// <summary>
    /// Gets or creates a <see cref="SemaphoreSlim"/> for the specified CP group identifier.
    /// </summary>
    /// <param name="groupId">The CP group identifier.</param>
    /// <returns>The <see cref="SemaphoreSlim"/> for the CP group.</returns>
    private SemaphoreSlim GetGroupSemaphore(CPGroupId groupId)
    {
        if (_groupSemaphores.TryGetValue(groupId, out var mutex))
            return mutex;

        var newMutex = new SemaphoreSlim(1, 1);
        var mostRecent = _groupSemaphores.GetOrAdd(groupId, newMutex);
        if (mostRecent != newMutex) newMutex.Dispose();
        return mostRecent;
    }

    #endregion

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        using var _ = await _lock.WriteLockAsync().CfAwait();
        if (!_disposed.InterlockedZeroToOne()) return;

        // shutdown - but don't invoke ShutdownAsync as CloseSessionAsync would throw,
        // this object is now disposed (we switched the flag literally one line above)
        _running = false;
        await _groupSessions.ParallelForEachAsync((entry, _) =>
        {
            var (groupId, session) = entry;
            InvalidateSession(groupId, session.Id);
            return RequestCloseSessionAsync(groupId, session.Id);
        }).CfAwait();

        // stop heartbeat
        _heartbeatCancel.Cancel();
        try
        {
            await _heartbeatTask.CfAwaitCanceled();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Caught an exception while disposing a CP Session Heartbeat.");
        }

        _heartbeatCancel.Dispose();

        // dispose what needs to be disposed
        foreach (var groupSemaphore in _groupSemaphores.Values)
            groupSemaphore.Dispose();

        _groupSessions.Clear();
        _groupSemaphores.Clear();
        _uniqueThreadIds.Clear();

        await _lock.DisposeAsync().CfAwait();
    }

    /// <summary>
    /// (for tests only)
    /// Returns the acquisitions count for the specified session.
    /// </summary>
    /// <param name="groupId">The CP group identifier.</param>
    /// <param name="sessionId">The CP session identifier.</param>
    /// <returns>The acquisitions count for the specified session.</returns>
    internal int GetAcquiredSessionCount(CPGroupId groupId, long sessionId)
    {
        ThrowIfDisposed();
        return _groupSessions.TryGetValue(groupId, out var sessionState) && sessionState.Id == sessionId
            ? sessionState.AcquireCount
            : 0;
    }
}
