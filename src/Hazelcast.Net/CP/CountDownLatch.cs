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

internal class CountDownLatch : CPDistributedObjectBase, ICountDownLatch
{
    public CountDownLatch(string name, CPGroupId groupId, Cluster cluster, SerializationService serializationService)
        : base(ServiceNames.CountDownLatch, name, groupId, cluster, serializationService)
    { }

    public async Task<bool> AwaitAsync(TimeSpan timeout)
    {
        var timeoutMillis = (long) timeout.TotalMilliseconds;
        if (timeoutMillis < 0) timeoutMillis = 0;
        using var requestMessage = CountDownLatchAwaitCodec.EncodeRequest(CPGroupId, Name, Guid.NewGuid(), timeoutMillis);
        using var responseMessage = await SendCPLeaderAsync(requestMessage).CfAwait();
        return CountDownLatchAwaitCodec.DecodeResponse(responseMessage).Response;
    }

    public async Task CountDownAsync()
    {
        var round = await GetRoundAsync().CfAwait();
        var uuid = Guid.NewGuid();
        for (;;)
        {
            try
            {
                await CountDownAsync(round, uuid).CfAwait();
                return;
            }
            catch (RemoteException e) when (e.Error == RemoteError.OperationTimeout)
            {
                // ignore and retry
            }
        }
    }

    private async Task<int> GetRoundAsync()
    {
        using var requestMessage = CountDownLatchGetRoundCodec.EncodeRequest(CPGroupId, Name);
        using var responseMessage = await SendCPLeaderAsync(requestMessage).CfAwait();
        return CountDownLatchGetRoundCodec.DecodeResponse(responseMessage).Response;
    }

    private async Task CountDownAsync(int round, Guid uuid)
    {
        using var requestMessage = CountDownLatchCountDownCodec.EncodeRequest(CPGroupId, Name, uuid, round);
        using var responseMessage = await SendCPLeaderAsync(requestMessage).CfAwait();
        CountDownLatchCountDownCodec.DecodeResponse(responseMessage);
    }

    public async Task<int> GetCountAsync()
    {
        using var requestMessage = CountDownLatchGetCountCodec.EncodeRequest(CPGroupId, Name);
        using var responseMessage = await SendCPLeaderAsync(requestMessage).CfAwait();
        return CountDownLatchGetCountCodec.DecodeResponse(responseMessage).Response;
    }

    public async Task<bool> TrySetCountAsync(int count)
    {
        if (count <= 0) throw new ArgumentException("Value must be greater than zero.", nameof(count));

        using var requestMessage = CountDownLatchTrySetCountCodec.EncodeRequest(CPGroupId, Name, count);
        using var responseMessage = await SendCPLeaderAsync(requestMessage).CfAwait();
        return CountDownLatchTrySetCountCodec.DecodeResponse(responseMessage).Response;
    }
}
