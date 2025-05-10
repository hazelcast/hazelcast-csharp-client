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
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.CP;

class CPMap<TKey, TValue> : CPDistributedObjectBase, ICPMap<TKey, TValue>
{
    public CPMap(string serviceName,
        string name,
        Cluster cluster,
        SerializationService serializationService,
        CPGroupId cpGroupId) : base(serviceName, name, cpGroupId, cluster, serializationService)
    { }

    public async Task<TValue> PutAsync(TKey key, TValue value)
    {
        var keyData = ToSafeData(key);
        var valueData = ToSafeData(value);
        var message = CPMapPutCodec.EncodeRequest(CPGroupId, Name, keyData, valueData);
        var response = await SendCPLeaderAsync(message).CfAwait();
        var responseData = CPMapPutCodec.DecodeResponse(response).Response;
        return await ToObjectAsync<TValue>(responseData).CfAwait();
    }

    public async Task SetAsync(TKey key, TValue value)
    {
        var keyData = ToSafeData(key);
        var valueData = ToSafeData(value);
        var message = CPMapSetCodec.EncodeRequest(CPGroupId, Name, keyData, valueData);
        var response = await SendCPLeaderAsync(message).CfAwait();
        _ = CPMapSetCodec.DecodeResponse(response);
    }

    public async Task<TValue> GetAsync(TKey key)
    {
        var keyData = ToSafeData(key);
        var message = CPMapGetCodec.EncodeRequest(CPGroupId, Name, keyData);
        var response = await SendCPLeaderAsync(message).CfAwait();
        var responseData = CPMapGetCodec.DecodeResponse(response).Response;
        return await ToObjectAsync<TValue>(responseData).CfAwait();
    }

    public async Task<TValue> RemoveAsync(TKey key)
    {
        var keyData = ToSafeData(key);
        var message = CPMapRemoveCodec.EncodeRequest(CPGroupId, Name, keyData);
        var response = await SendCPLeaderAsync(message).CfAwait();
        var responseData = CPMapRemoveCodec.DecodeResponse(response).Response;
        return await ToObjectAsync<TValue>(responseData).CfAwait();
    }

    public async Task DeleteAsync(TKey key)
    {
        var keyData = ToSafeData(key);
        var message = CPMapDeleteCodec.EncodeRequest(CPGroupId, Name, keyData);
        var response = await SendCPLeaderAsync(message).CfAwait();
        _ = CPMapDeleteCodec.DecodeResponse(response);
    }

    public async Task<bool> CompareAndSetAsync(TKey key, TValue expectedValue, TValue newValue)
    {
        var keyData = ToSafeData(key);
        var expectedValueData = ToSafeData(expectedValue);
        var newValueData = ToSafeData(newValue);
        var message = CPMapCompareAndSetCodec.EncodeRequest(CPGroupId, Name, keyData, expectedValueData, newValueData);
        var response = await SendCPLeaderAsync(message).CfAwait();
        return CPMapCompareAndSetCodec.DecodeResponse(response).Response;
    }

}
