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
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Protocol.Models;

namespace Hazelcast.CP;

internal class SessionAwareSemaphore : CPDistributedObjectBase, ISemaphore
{
    public const int DrainSessionAcqCount = 1024;

    private readonly CPSessionManager _sessionManager;

    public SessionAwareSemaphore(string name, CPGroupId groupId, Cluster cluster, CPSessionManager sessionManager)
        : base(ServiceNames.Semaphore, name, groupId, cluster)
    {
        _sessionManager = sessionManager;
    }

    // this semaphore is session-aware, therefore it can use a plain "thread id" in the
    // client request messages - and for semaphores, this has to be a somewhat not constant
    // yet not totally random number, which is used internally by the cluster to manage
    // queues and stuff - but, two simultaneous operations on the same "thread id" can
    // interrupt each other, so this id has to somehow represent the .NET async flow (cannot
    // be the plain .NET managed thread) and therefore we use the AsyncContext.
    private static long GetThreadId() => AsyncContext.Current.Id;

    public async Task<bool> InitializeAsync(int permits = 1)
    {
        permits.ThrowIfLessThanOrZero(nameof(permits));

        var requestMessage = SemaphoreInitCodec.EncodeRequest(CPGroupId, Name, permits);
        var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
        return SemaphoreInitCodec.DecodeResponse(responseMessage).Response;
    }

    public async Task AcquireAsync(int permits = 1)
    {
        permits.ThrowIfLessThanOrZero(nameof(permits));
        var invocationUid = Guid.NewGuid();
        var threadId = GetThreadId();

        for (;;)
        {
            var sessionId = await _sessionManager.AcquireSessionAsync(CPGroupId, permits).CfAwait();

            try
            {
                var requestMessage = SemaphoreAcquireCodec.EncodeRequest(
                    CPGroupId, Name, sessionId, threadId, invocationUid, permits,
                    -1);
                var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
                _ = SemaphoreAcquireCodec.DecodeResponse(responseMessage);
                return; // cannot fail, since timeout -1 was specified
            }
            catch (RemoteException e) when (e.Error == RemoteError.SessionExpiredException)
            {
                _sessionManager.InvalidateSession(CPGroupId, sessionId);
                // try again
            }
            catch (RemoteException e) when (e.Error == RemoteError.WaitKeyCancelledException)
            {
                _sessionManager.ReleaseSession(CPGroupId, sessionId, permits);
                throw new InvalidOperationException(
                    $"Could not acquire semaphore {Name} because the acquisition operation was cancelled, possibly because of another operation.");
            }
            catch (Exception)
            {
                _sessionManager.ReleaseSession(CPGroupId, sessionId, permits);
                throw;
            }
        }
    }

    public async Task<bool> TryAcquireAsync(int permits = 1, long timeoutMs = 0)
    {
        permits.ThrowIfLessThanOrZero(nameof(permits));
        if (timeoutMs < 0) timeoutMs = 0;
        var invocationUid = Guid.NewGuid();
        var countdown = new Countdown(timeoutMs);
        var threadId = GetThreadId();

        for (;;)
        {
            var sessionId = await _sessionManager.AcquireSessionAsync(CPGroupId, permits).CfAwait();

            try
            {
                var requestMessage = SemaphoreAcquireCodec.EncodeRequest(
                    CPGroupId, Name, sessionId, threadId, invocationUid, permits, 
                    countdown.RemainingMilliseconds);
                var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
                var acquired = SemaphoreAcquireCodec.DecodeResponse(responseMessage).Response;
                if (!acquired) _sessionManager.ReleaseSession(CPGroupId, sessionId, permits);
                return acquired;
            }
            catch (RemoteException e) when (e.Error == RemoteError.SessionExpiredException)
            {
                _sessionManager.InvalidateSession(CPGroupId, sessionId);
                if (countdown.Elapsed) return false; // else try again
            }
            catch (RemoteException e) when (e.Error == RemoteError.WaitKeyCancelledException)
            {
                _sessionManager.ReleaseSession(CPGroupId, sessionId, permits);
                return false;
            }
            catch (Exception)
            {
                _sessionManager.ReleaseSession(CPGroupId, sessionId, permits);
                throw;
            }
        }
    }

    public async Task ReleaseAsync(int permits = 1)
    {
        permits.ThrowIfLessThanOrZero(nameof(permits));
        var invocationUid = Guid.NewGuid();
        var threadId = GetThreadId();

        var sessionId = _sessionManager.GetSessionId(CPGroupId);
        if (sessionId == CPSessionManager.NoSessionId)
            throw new InvalidOperationException("No CP session.");

        try
        {
            var requestMessage = SemaphoreReleaseCodec.EncodeRequest(CPGroupId, Name, sessionId, threadId, invocationUid, permits);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            _ = SemaphoreReleaseCodec.DecodeResponse(responseMessage);
        }
        catch (RemoteException e) when (e.Error == RemoteError.SessionExpiredException)
        {
            _sessionManager.InvalidateSession(CPGroupId, sessionId);
            throw;
        }
        finally
        {
            _sessionManager.ReleaseSession(CPGroupId, sessionId, permits);
        }
    }

    public async Task<int> GetAvailablePermitsAsync()
    {
        var requestMessage = SemaphoreAvailablePermitsCodec.EncodeRequest(CPGroupId, Name);
        var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
        return SemaphoreAvailablePermitsCodec.DecodeResponse(responseMessage).Response;
    }

    public async Task<int> DrainPermitsAsync()
    {
        var invocationUid = Guid.NewGuid();
        var threadId = GetThreadId();

        for (;;)
        {
            var sessionId = await _sessionManager.AcquireSessionAsync(CPGroupId, DrainSessionAcqCount).CfAwait();
            try
            {
                var requestMessage = SemaphoreDrainCodec.EncodeRequest(CPGroupId, Name, sessionId, threadId, invocationUid);
                var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
                var count = SemaphoreDrainCodec.DecodeResponse(responseMessage).Response;
                _sessionManager.ReleaseSession(CPGroupId, sessionId, DrainSessionAcqCount - count);
                return count;
            }
            catch (RemoteException e) when (e.Error == RemoteError.SessionExpiredException)
            {
                _sessionManager.InvalidateSession(CPGroupId, sessionId);
            }
            catch (Exception)
            {
                _sessionManager.ReleaseSession(CPGroupId, sessionId, DrainSessionAcqCount);
                throw;
            }
        }
    }

    public Task ReducePermitsAsync(int delta) => ChangePermits(-delta.ThrowIfLessThanZero());

    public Task IncreasePermitsAsync(int delta) => ChangePermits(delta.ThrowIfLessThanZero());

    private async Task ChangePermits(int delta)
    {
        var invocationUid = Guid.NewGuid();
        var threadId = GetThreadId();

        var sessionId = await _sessionManager.AcquireSessionAsync(CPGroupId).CfAwait();

        try
        {
            var requestMessage = SemaphoreChangeCodec.EncodeRequest(CPGroupId, Name, sessionId, threadId, invocationUid, delta);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            _ = SemaphoreChangeCodec.DecodeResponse(responseMessage);
        }
        catch (RemoteException e) when (e.Error == RemoteError.SessionExpiredException)
        {
            _sessionManager.InvalidateSession(CPGroupId, sessionId);
            throw;
        }
        finally
        {
            _sessionManager.ReleaseSession(CPGroupId, sessionId);
        }
    }

    public override async ValueTask DestroyAsync()
    {
        var requestMessage = CPGroupDestroyCPObjectCodec.EncodeRequest(CPGroupId, ServiceNames.Semaphore, Name);
        await Cluster.Messaging.SendAsync(requestMessage).CfAwaitNoThrow();
    }
}
