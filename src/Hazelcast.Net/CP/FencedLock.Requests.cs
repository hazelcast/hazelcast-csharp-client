// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.CP
{
    /// <summary>
    /// Implementation of server requests for <see cref="FencedLock"/>
    /// </summary>
    internal partial class FencedLock
    {
        protected async Task<long> RequestLockAsync(long sessionId, long threadId, Guid invocationId)
        {
            var requestMessage = FencedLockLockCodec.EncodeRequest(CPGroupId, Name, sessionId, threadId, invocationId);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = FencedLockLockCodec.DecodeResponse(responseMessage);
            return response.Response;
        }

        protected async Task<long> RequestTryLockAsync(long sessionId, long threadId, Guid invocationId, long timeoutMillisecond)
        {
            var requestMessage = FencedLockTryLockCodec.EncodeRequest(CPGroupId, Name, sessionId, threadId, invocationId, timeoutMillisecond);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = FencedLockTryLockCodec.DecodeResponse(responseMessage);
            return response.Response;
        }

        protected async Task<bool> RequestUnlockAsync(long sessionId, long threadId, Guid invocationId)
        {
            var requestMessage = FencedLockUnlockCodec.EncodeRequest(CPGroupId, Name, sessionId, threadId, invocationId);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = FencedLockUnlockCodec.DecodeResponse(responseMessage);
            return response.Response;
        }

        protected async Task RequestDestroyAsync()
        {
            var requestMessage = CPGroupDestroyCPObjectCodec.EncodeRequest(CPGroupId, ServiceName, Name);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var _ = CPGroupDestroyCPObjectCodec.DecodeResponse(responseMessage);
        }

        protected async Task<FencedLock.LockOwnershipState> RequestLockOwnershipStateAsync()
        {
            var requestMessage = FencedLockGetLockOwnershipCodec.EncodeRequest(CPGroupId, Name);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = FencedLockGetLockOwnershipCodec.DecodeResponse(responseMessage);
            return new FencedLock.LockOwnershipState(response.Fence, response.SessionId, response.ThreadId, response.LockCount);
        }
    }
}
