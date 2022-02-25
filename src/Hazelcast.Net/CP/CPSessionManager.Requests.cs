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
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.CP
{
    /// <summary>
    /// Implements server side requests for CP Session
    /// </summary>
    internal partial class CPSessionManager
    {
        /// <summary>
        /// Generates a cluster-wide unique thread id for the caller
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        internal async Task<long> RequestGenerateThreadIdAsync(CPGroupId groupId)
        {
            var requestMessage = CPSessionGenerateThreadIdCodec.EncodeRequest(groupId);
            var responseMessage = await _cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = CPSessionGenerateThreadIdCodec.DecodeResponse(responseMessage);
            return response.Response;
        }

        /// <summary>
        /// Creates a new session on the server
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns>CPSubsystemSessionState and heartbeat milliseconds</returns>
        internal async Task<(CPSubsystemSessionState, long)> RequestNewSessionAsync(CPGroupId groupId)
        {
            var requestMessage = CPSessionCreateSessionCodec.EncodeRequest(groupId, _cluster.ClientId.ToString());
            var responseMessage = await _cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = CPSessionCreateSessionCodec.DecodeResponse(responseMessage);
            return (new CPSubsystemSessionState(response.SessionId, response.TtlMillis), response.HeartbeatMillis);
        }

        /// <summary>
        /// Closes the session on the server
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        internal async Task<bool> RequestCloseSessionAsync(CPGroupId groupId, long sessionId)
        {
            var requestMessage = CPSessionCloseSessionCodec.EncodeRequest(groupId, sessionId);
            var responseMessage = await _cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = CPSessionCloseSessionCodec.DecodeResponse(responseMessage);
            return response.Response;
        }


        /// <summary>
        /// Sends heartbeat for a given session
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        internal async Task RequestSessionHeartbeat(CPGroupId groupId, long sessionId)
        {
            var requestMessage = CPSessionHeartbeatSessionCodec.EncodeRequest(groupId, sessionId);
            var responseMessage = await _cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var _ = CPSessionHeartbeatSessionCodec.DecodeResponse(responseMessage);
        }
    }
}
