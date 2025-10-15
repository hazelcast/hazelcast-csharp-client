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
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Protocol.Models;
using Hazelcast.Serialization;

namespace Hazelcast.CP;

internal class SessionLessSemaphore : CPDistributedObjectBase, ISemaphore
{
    private readonly CPSessionManager _sessionManager;

    public SessionLessSemaphore(string name, CPGroupId groupId, Cluster cluster, SerializationService serializationService, CPSessionManager sessionManager) 
        : base(ServiceNames.Semaphore, name, groupId, cluster, serializationService)
    {
        _sessionManager = sessionManager;
    }

    // this semaphore is session-less, and cannot simply use a plain "thread id" in
    // the client request messages - as there can be other threads with that same id
    // in other clients - therefore we have to request a unique "thread id" from the
    // cluster, which is identified by (group id, local thread id) - now for local
    // thread id, it has to be a somewhat not constant yet not totally random number,
    // which is used internally by the cluster to manage queues and stuff - but, two
    // simultaneous operations on the same "thread id" can interrupt each other, so
    // this id has to somehow represent the .NET async flow (cannot be the plain .NET
    // managed thread) and therefore we use the AsyncContext.
    private ValueTask<long> GetThreadId() => _sessionManager.GetOrCreateUniqueThreadIdAsync(CPGroupId, AsyncContext.Current.Id);

    public async Task<bool> InitializeAsync(int permits = 1)
    {
        permits.ThrowIfLessThanOrZero(nameof(permits));

        var requestMessage = SemaphoreInitCodec.EncodeRequest(CPGroupId, Name, permits);
        var responseMessage = await SendCPLeaderAsync(requestMessage).CfAwait();
        return SemaphoreInitCodec.DecodeResponse(responseMessage).Response;
    }

    public async Task AcquireAsync(int permits = 1)
    {
        await TryAcquireAsync(permits, -1).CfAwait();
    }

    public async Task<bool> TryAcquireAsync(int permits = 1, long timeoutMs = 0)
    {
        permits.ThrowIfLessThanOrZero(nameof(permits));
        if (timeoutMs < 0) timeoutMs = 0;
        var invocationUid = Guid.NewGuid();
        var threadId = await GetThreadId().CfAwait();

        var requestMessage = SemaphoreAcquireCodec.EncodeRequest(
            CPGroupId, Name, CPSessionManager.NoSessionId, threadId, invocationUid, permits,
            timeoutMs);

        try
        {
            var responseMessage = await SendCPLeaderAsync(requestMessage).CfAwait();
            return SemaphoreAcquireCodec.DecodeResponse(responseMessage).Response;
        }
        catch (RemoteException e) when (e.Error == RemoteError.WaitKeyCancelledException)
        {
            throw new InvalidOperationException(
                $"Could not acquire semaphore {Name} because the acquisition operation was cancelled, possibly because of another operation.");
        }
    }

    public async Task ReleaseAsync(int permits = 1)
    {
        permits.ThrowIfLessThanOrZero(nameof(permits));
        var invocationUid = Guid.NewGuid();
        var threadId = await GetThreadId().CfAwait();

        var requestMessage = SemaphoreReleaseCodec.EncodeRequest(CPGroupId, Name, CPSessionManager.NoSessionId, threadId, invocationUid, permits);

        try
        {
            var responseMessage = await SendCPLeaderAsync(requestMessage).CfAwait();
            _ = SemaphoreReleaseCodec.DecodeResponse(responseMessage);
        }
        catch (RemoteException e) when (e.Error == RemoteError.WaitKeyCancelledException)
        {
            throw new InvalidOperationException(
                $"Could not release semaphore {Name} because the release operation was cancelled, possibly because of another operation.");
        }
    }

    public async Task<int> GetAvailablePermitsAsync()
    {
        var requestMessage = SemaphoreAvailablePermitsCodec.EncodeRequest(CPGroupId, Name);
        var responseMessage = await SendCPLeaderAsync(requestMessage).CfAwait();
        return SemaphoreAvailablePermitsCodec.DecodeResponse(responseMessage).Response;
    }

    public async Task<int> DrainPermitsAsync()
    {
        var invocationUid = Guid.NewGuid();
        var threadId = await GetThreadId().CfAwait();

        var requestMessage = SemaphoreDrainCodec.EncodeRequest(CPGroupId, Name, CPSessionManager.NoSessionId, threadId, invocationUid);
        var responseMessage = await SendCPLeaderAsync(requestMessage).CfAwait();
        return SemaphoreDrainCodec.DecodeResponse(responseMessage).Response;
    }

    public Task ReducePermitsAsync(int delta) => ChangePermits(-delta.ThrowIfLessThanZero());

    public Task IncreasePermitsAsync(int delta) => ChangePermits(delta.ThrowIfLessThanZero());

    private async Task ChangePermits(int delta)
    {
        var invocationUid = Guid.NewGuid();
        var threadId = await GetThreadId().CfAwait();

        var requestMessage = SemaphoreChangeCodec.EncodeRequest(CPGroupId, Name, CPSessionManager.NoSessionId, threadId, invocationUid, delta);

        try
        {
            var responseMessage = await SendCPLeaderAsync(requestMessage).CfAwait();
            _ = SemaphoreChangeCodec.DecodeResponse(responseMessage);
        }
        catch (RemoteException e) when (e.Error == RemoteError.WaitKeyCancelledException)
        {
            throw new InvalidOperationException(
                $"Could not change semaphore {Name} because the change operation was cancelled, possibly because of another operation.");
        }
    }
}