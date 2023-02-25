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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncKeyedLock;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Microsoft.Extensions.Logging;

namespace Hazelcast.CP
{
    /// <summary>
    /// Manages server side cp session requests and heartbeat
    /// </summary>
    internal partial class CPSessionManager : IAsyncDisposable
    {
        #region Properties
        /// <summary>
        /// SemaphoreSlim is used altough java client uses ReaderWriterLockSlim
        /// <para>
        /// Reason: "ReaderWriterLockSlim has managed thread affinity; that is, each Thread object must make its
        /// own method calls to enter and exit lock modes. No thread can change the mode of another thread."
        /// </para>
        /// <seealso  href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.readerwriterlockslim?view=net-6.0#remarks"/>
        /// </summary>
        private readonly SemaphoreSlim _semaphoreReadWrite = new SemaphoreSlim(1, 1);
        private AsyncKeyedLocker<CPGroupId> _groupIdSemaphores;
        private readonly ConcurrentDictionary<CPGroupId, CPSession> _sessions = new ConcurrentDictionary<CPGroupId, CPSession>();
        private int _disposed;
        private readonly object _mutex = new object();
        private readonly ILogger _logger;
        private readonly Cluster _cluster;

        public const long NoSessionId = -1;
        #endregion

        #region SessionManagement
        public CPSessionManager(Cluster cluster)
        {
            Reset();
            _cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            _logger = _cluster.State.LoggerFactory.CreateLogger<CPSessionManager>();
            _cancel = new CancellationTokenSource();
            _heartbeating = Task.CompletedTask;
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
                IEnumerable<(CPGroupId, CPSession)> sessions;
                lock (_mutex) sessions = _sessions.Select(p => (p.Key, p.Value));

                var tasks = new List<Task>();
                int taskCount = 4;
                var enumerator = sessions.GetEnumerator();

                void StartCurrent()
                {
                    var currentTask = CloseSessionAsync(enumerator.Current.Item1, enumerator.Current.Item2.Id);
                    tasks.Add(currentTask);
                }

                //Start tasks as much as possible.
                while (tasks.Count < taskCount && enumerator.MoveNext() && !_cancel.Token.IsCancellationRequested)
                    StartCurrent();

                // when a tasks completes, try to add next one.
                while (tasks.Count > 0)
                {
                    var completed = await Task.WhenAny(tasks).CfAwait();
                    tasks.Remove(completed);

                    if (enumerator.MoveNext() && !_cancel.Token.IsCancellationRequested)
                        StartCurrent();
                }
            }
            finally { _semaphoreReadWrite.Release(); }
        }

        /// <summary>
        /// Gets or createas a session if absent by group id.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns><see cref="CPSubsystemSessionState"/></returns>
        /// <exception cref="HazelcastInstanceNotActiveException"></exception>
        private async Task<CPSession> GetOrCreateSessionAsync(CPGroupId groupId)
        {
            await _semaphoreReadWrite.WaitAsync().CfAwait();

            try
            {
                if (_disposed == 1) throw new ObjectDisposedException("CP Subsystem Session is already disposed.");

                if (_sessions.TryGetValue(groupId, out var sessionState) && sessionState.IsValid)
                {
                    return sessionState;
                }
                else
                {
                    // Wait and lock only for the groupId
                    using (await _groupIdSemaphores.LockAsync(groupId).CfAwait())
                    {
                        // check once more after groupId semaphore
                        if (_sessions.TryGetValue(groupId, out sessionState) && sessionState.IsValid)
                        {
                            return sessionState;
                        }

                        var session = await RequestNewSessionAsync(groupId).CfAwait();
                        _sessions[groupId] = session.Item1;
                        ScheduleHeartbeat(TimeSpan.FromMilliseconds(session.Item2));
                        return session.Item1;
                    }
                }

            }
            finally { _semaphoreReadWrite.Release(); }
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
                _groupIdSemaphores = new AsyncKeyedLocker<CPGroupId>(o =>
                {
                    o.PoolSize = 20;
                    o.PoolInitialFill = 1;
                });

                _sessions.Clear();
            }
        }
        #endregion
    }
}
