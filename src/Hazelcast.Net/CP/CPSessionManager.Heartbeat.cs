// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Models;
using Microsoft.Extensions.Logging;

namespace Hazelcast.CP
{
    /// <summary>
    /// Implements Heartbeat side of CP Session
    /// </summary>
    internal partial class CPSessionManager
    {
        /// <summary>
        /// Ensures that heartbeat is running by starting it if it is not already running.
        /// </summary>
        internal void EnsureHeartbeat(TimeSpan period)
        {
            if (_heartbeatRunning.InterlockedZeroToOne())
                _heartbeatTask = BeatAsync(period, _heartbeatCancel.Token);
        }

        /// <summary>
        /// Runs the heartbeat background task.
        /// </summary>
        private async Task BeatAsync(TimeSpan period, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(period, cancellationToken).CfAwait();
                    if (cancellationToken.IsCancellationRequested) break;
                    await RunAsync(cancellationToken).CfAwait();
                }
            }
            catch (Exception)
            {
                // exception observed
            }
        }

        /// <summary>
        /// Runs for all sessions.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        private async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Run CP Session Heartbeat");

            // capture sessions
            List<KeyValuePair<CPGroupId, CPSession>> sessions;
            using (var _ = await _lock.ReadLockAsync(/*cancellationToken*/))
            {
                sessions = new List<KeyValuePair<CPGroupId, CPSession>>(_groupSessions);
            }

            await sessions.ParallelForEachAsync((entry, token) =>
            {
                var (groupId, session) = entry;
                return session.IsInUse 
                    ? BeatSessionAsync(groupId, session)
                    : default;
            }, cancellationToken);
        }

        /// <summary>
        /// Runs for a session.
        /// </summary>
        /// <param name="groupId">The CP group identifier.</param>
        /// <param name="session">The CP session.</param>
        private async Task BeatSessionAsync(CPGroupId groupId, CPSession session)
        {
            try
            {
                await RequestSessionHeartbeat(groupId, session.Id).CfAwait();
            }
            catch (Exception e)
            {
                if (e is RemoteException { Error: RemoteError.SessionExpiredException } or 
                         RemoteException { Error: RemoteError.CpGroupDestroyedException })
                {
                    InvalidateSession(groupId, session.Id);
                }

                _logger.LogWarning(e, "CP Session Heartbeat has thrown an exception, but will continue.");
            }
        }
    }
}
