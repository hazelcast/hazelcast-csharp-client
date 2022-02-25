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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Microsoft.Extensions.Logging;

namespace Hazelcast.CP
{
    /// <summary>
    /// Implements Heartbeat side of CP Session
    /// </summary>
    internal partial class CPSubsystemSession
    {
        private readonly CancellationTokenSource _cancel; // initialized in ctor
        private Task _heartbeating;
        private int _heartbeatState;

        /// <summary>
        /// Schedules the session heartbeat requests. It does not require to check state of the heartbeat
        /// <para>Use only the method to run heartbeat.</para>
        /// </summary>
        internal void ScheduleHeartbeat(TimeSpan period)
        {
            if (_disposed == 0 && Interlocked.CompareExchange(ref _heartbeatState, 1, 0) == 0)
            {
                _heartbeating = BeatAsync(period, _cancel.Token);
            }
        }

        private async Task BeatAsync(TimeSpan period, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(period, cancellationToken).CfAwait();

                if (cancellationToken.IsCancellationRequested) break;

                await RunAllAsync(cancellationToken).CfAwait();
            }
        }

        /// <summary>
        /// Runs for all sessions
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task RunAllAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Run CP Session Heartbeat");

            lock (_mutex) if (_sessions.IsEmpty) return;

            IEnumerable<(CPGroupId, CPSubsystemSessionState)> sessions;
            lock (_mutex) sessions = _sessions.Select(p => (p.Key, p.Value));

            await sessions.ParallelForEachAsync((p, cancellationToken) =>
            {
                if (!p.Item2.IsInUse) return default;
                return RunAsync(p.Item1, p.Item2, cancellationToken);

            }, cancellationToken).CfAwait();

        }

        /// <summary>
        /// Runs for a given session
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="sessionState"></param>
        /// <param name="cancellationToken"></param>        
        private async Task RunAsync(CPGroupId groupId, CPSubsystemSessionState sessionState, CancellationToken cancellationToken)
        {
            try
            {
                await RequestSessionHeartbeat(groupId, sessionState.Id).CfAwait();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "CP Session Heartbeat has thrown an exception, but will continue.");
            }
        }
    }
}
