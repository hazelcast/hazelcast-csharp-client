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
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Microsoft.Extensions.Logging;

namespace Hazelcast.CP
{
    /// <summary>
    /// Cp Subsystem Session manages server side session requests and heartbeat
    /// </summary>
    internal partial class CPSubsystemSession : IAsyncDisposable
    {
        #region Properties
        private readonly ConcurrentDictionary<CPGroupId, SemaphoreSlim> _groupIdSemaphores = new ConcurrentDictionary<CPGroupId, SemaphoreSlim>();
        private readonly ConcurrentDictionary<CPGroupId, CPSubsystemSessionState> _sessions = new ConcurrentDictionary<CPGroupId, CPSubsystemSessionState>();
        private int _disposed;
        private readonly object _mutex = new object();
        private readonly SemaphoreSlim _semaphoreReadWrite = new SemaphoreSlim(1, 1);
        private readonly ILogger _logger;
        private readonly Cluster Cluster;

        public const int NoSessionId = -1;
        #endregion

        #region SessionManagement
        public CPSubsystemSession(Cluster cluster)
        {
            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            _logger = Cluster.State.LoggerFactory.CreateLogger<CPSubsystemSession>();
            _cancel = new CancellationTokenSource();
            HConsole.Configure(x => x.Configure<Heartbeat>().SetPrefix("CP.SESSION"));
        }

        /// <summary>
        /// Acquires the session by increasing given count, creates if absent.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="count">Increase count of acquirment</param>
        /// <returns>Session Id</returns>
        public async Task<long> AcquireSessionAsync(CPGroupId groupId, int count = 1)
        {
            var session = await GetOrCreateSessionAsync(groupId).CfAwait();
            session.Acquire(count);
            return session.Id;
        }

        /// <summary>
        /// Releases the session by decreasing given count
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="sessionId"></param>
        /// <param name="count">Decrease count of release</param>
        /// <returns></returns>
        public void ReleaseSession(CPGroupId groupId, long sessionId, int count = 1)
        {
            if (_sessions.TryGetValue(groupId, out var sessionState) && sessionState.Id == sessionId)
            {
                sessionState.Release(count);
            }
        }

        /// <summary>
        /// Invalidates the given session after invalidation no more heartbeat will be sent.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="sessionId"></param>
        public void InvalidateSession(CPGroupId groupId, long sessionId)
        {
            if (_sessions.TryGetValue(groupId, out var sessionState) && sessionState.Id == sessionId)
            {
                _sessions.TryRemove(groupId, sessionState);
            }
        }

        /// <summary>
        /// Gets session id by given group id
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns>Session id or <see cref="NoSessionId"/> if absent</returns>
        public long GetSessionId(CPGroupId groupId)
        {
            if (_sessions.TryGetValue(groupId, out var sessionState))
                return sessionState.Id;
            else
                return NoSessionId;
        }

        public async Task CloseSessionAsync(CPGroupId groupId, long sessionId)
        {
            InvalidateSession(groupId, sessionId);
            await RequestCloseSessionAsync(groupId, sessionId).CfAwait();
        }

        /// <summary>
        /// Shuts down sessions on server and disposes
        /// </summary>
        /// <returns></returns>
        public async Task ShutdownAsync()
        {
            await _semaphoreReadWrite.WaitAsync().CfAwait();

            try
            {
                await _sessions.Keys
                    .ParallelForEachAsync(async (key, cancelToken) =>
                        {
                            try
                            {
                                await CloseSessionAsync(key, _sessions[key].Id).CfAwait();
                            }
                            catch (Exception e)
                            {
                                _logger.LogWarning("Exception thrown while closing CP sessions", e);
                            }
                        },
                        _cancel.Token)
                    .CfAwait();

                await DisposeAsync().CfAwait();
            }
            finally { _semaphoreReadWrite.Release(); }
        }

        /// <summary>
        /// Gets or createas a session if absent by group id.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns><see cref="CPSubsystemSessionState"/></returns>
        /// <exception cref="HazelcastInstanceNotActiveException"></exception>
        private async Task<CPSubsystemSessionState> GetOrCreateSessionAsync(CPGroupId groupId)
        {
            await _semaphoreReadWrite.WaitAsync().CfAwait();

            try
            {
                if (_disposed == 1) throw new ObjectDisposedException("CP Subsystem Session is already disposed!");

                if (_sessions.TryGetValue(groupId, out var sessionState) && sessionState.IsValid)
                {
                    return sessionState;
                }
                else
                {
                    // Wait and lock only for the groupId
                    var semaphore = GetSemaphoreBy(groupId);
                    await semaphore.WaitAsync().CfAwait();

                    try
                    {
                        var session = await RequestNewSessionAsync(groupId).CfAwait();
                        ScheduleHeartbeat(TimeSpan.FromMilliseconds(session.Item2));
                        _sessions[groupId] = session.Item1;
                        return session.Item1;
                    }
                    catch (Exception e)
                    {
                        throw;
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }

            }
            finally { _semaphoreReadWrite.Release(); }
        }

        /// <summary>
        /// Gets or create a <see cref="SemaphoreSlim"/> for given group id
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns><see cref="SemaphoreSlim"/></returns>
        private SemaphoreSlim GetSemaphoreBy(CPGroupId groupId)
        {
            if (_groupIdSemaphores.TryGetValue(groupId, out var mutex))
                return mutex;

            var newMutex = new SemaphoreSlim(1, 1);
            _groupIdSemaphores[groupId] = newMutex;
            return newMutex;
        }
        #endregion

        #region Dispose&Clear
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            //Dispose heartbeat
            Interlocked.Exchange(ref _heartbeatState, 0);

            Reset();
            await ShutdownAsync().CfAwait();

            _cancel.Cancel();

            try
            {
                await _heartbeating.CfAwaitCanceled();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Caught an exception while disposing CP Session Heartbeat.");
            }

            _cancel.Dispose();
            _semaphoreReadWrite.Dispose();
        }


        /// <summary>
        /// Returns acquired session count. For testing purpose.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        internal int GetAcquiredSessionCount(CPGroupId groupId, long sessionId)
        {
            if (_sessions.TryGetValue(groupId, out var sessionState) && sessionState.Id == sessionId)
            {
                return sessionState.AcquireCount;
            }

            return 0;
        }

        /// <summary>
        /// Resets internal states
        /// </summary>
        public void Reset()
        {
            lock (_mutex)
            {
                foreach (var semaphore in _groupIdSemaphores.Values)
                    semaphore.Dispose();

                _groupIdSemaphores.Clear();

                _sessions.Clear();
            }
        }
        #endregion
    }
}
