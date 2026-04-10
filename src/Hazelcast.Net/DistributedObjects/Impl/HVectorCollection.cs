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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Models;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;
namespace Hazelcast.DistributedObjects.Impl
{
    internal class HVectorCollection<TKey, TVal> : DistributedObjectBase, IHVectorCollection<TKey, TVal>
    {
        public HVectorCollection(string name,
            DistributedObjectFactory factory,
            Cluster cluster,
            SerializationService serializationService,
            ILoggerFactory loggerFactory)
            : base(ServiceNames.VectorCollection, name, factory, cluster, serializationService, loggerFactory)
        { }

        public async Task<IVectorDocument<TVal>> GetAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            using var message = VectorCollectionGetCodec.EncodeRequest(Name, keyData);
            using var response = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(message, keyData).CfAwait();
            var rawResponse = VectorCollectionGetCodec.DecodeResponse(response).Value;
            return await DeserializeIVectorDocumentAsync<TVal>(rawResponse).CfAwait();
        }


        public async Task<IVectorDocument<TVal>> PutAsync(TKey key, IVectorDocument<TVal> valueIVectorDocument)
        {
            var (dataKey, rawDocument) = PrepareForPut(key, valueIVectorDocument);

            using var message = VectorCollectionPutCodec.EncodeRequest(Name, dataKey, rawDocument);
            using var response = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(message, dataKey).CfAwait();
            var rawResponse = VectorCollectionPutCodec.DecodeResponse(response).Value;
            return await DeserializeIVectorDocumentAsync<TVal>(rawResponse).CfAwait();
        }

        public async Task SetAsync(TKey key, IVectorDocument<TVal> vectorDocument)
        {
            var (dataKey, rawDocument) = PrepareForPut(key, vectorDocument);
            using var message = VectorCollectionSetCodec.EncodeRequest(Name, dataKey, rawDocument);
            using var _ = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(message, dataKey).CfAwait();
        }
        public async Task<IVectorDocument<TVal>> PutIfAbsentAsync(TKey key, IVectorDocument<TVal> IVectorDocument)
        {
            var (dataKey, rawDocument) = PrepareForPut(key, IVectorDocument);
            using var message = VectorCollectionPutIfAbsentCodec.EncodeRequest(Name, dataKey, rawDocument);
            using var response = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(message, dataKey).CfAwait();
            var rawResponse = VectorCollectionPutIfAbsentCodec.DecodeResponse(response).Value;
            return await DeserializeIVectorDocumentAsync<TVal>(rawResponse).CfAwait();
        }
        public async Task PutAllAsync([NotNull] IDictionary<TKey, IVectorDocument<TVal>> vectorDocumentMap)
        {
            vectorDocumentMap.ThrowIfNull();
            var entriesByPartition = new Dictionary<int, List<KeyValuePair<IData, IVectorDocument<IData>>>>();

            foreach (var kvp in vectorDocumentMap)
            {
                var key = kvp.Key ?? throw new ArgumentException($"Key cannot be null in {nameof(vectorDocumentMap)}.");
                var val = kvp.Value ?? throw new ArgumentException($"Value cannot be null in {nameof(vectorDocumentMap)}.");
                var(dataKey, rawDocument) = PrepareForPut(key, val);
                var partitionId = Cluster.Partitioner.GetPartitionId(dataKey.PartitionHash);

                if (entriesByPartition.TryGetValue(partitionId, out var list))
                {
                    list.Add(new KeyValuePair<IData, IVectorDocument<IData>>(dataKey, rawDocument));
                }
                else
                {
                    entriesByPartition[partitionId] = new List<KeyValuePair<IData, IVectorDocument<IData>>>()
                    {
                        new KeyValuePair<IData, IVectorDocument<IData>>(dataKey, rawDocument)
                    };
                }
            }

            var tasks = new List<Task<ClientMessage>>();
            foreach (var entry in entriesByPartition)
            {
#pragma warning disable CA2000 // ClientMessage ownership transferred to SendToPartitionOwnerAsync (fire-and-forget per partition)
                var message = VectorCollectionPutAllCodec.EncodeRequest(Name, entry.Value);
                tasks.Add(Cluster.Messaging.SendToPartitionOwnerAsync(message, entry.Key));
#pragma warning restore CA2000
            }

            await Task.WhenAll(tasks).CfAwait();
            foreach (var task in tasks)
                (await task.CfAwait()).Dispose();
        }
        public async Task<IVectorDocument<TVal>> RemoveAsync(TKey key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            var dataKey = ToSafeData(key);
            using var message = VectorCollectionRemoveCodec.EncodeRequest(Name, dataKey);
            using var response = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(message, dataKey).CfAwait();
            var rawResponse = VectorCollectionRemoveCodec.DecodeResponse(response).Value;
            return await DeserializeIVectorDocumentAsync<TVal>(rawResponse).CfAwait();
        }


        public async Task OptimizeAsync()
        {
            using var message = VectorCollectionOptimizeCodec.EncodeRequest(Name, null, Guid.NewGuid());
            using var _ = await Cluster.Messaging.SendAsync(message).CfAwait();
        }

        public async Task OptimizeAsync(string indexName)
        {
            using var message = VectorCollectionOptimizeCodec.EncodeRequest(Name, indexName, Guid.NewGuid());
            using var _ = await Cluster.Messaging.SendAsync(message).CfAwait();
        }
        public async Task ClearAsync()
        {
            using var message = VectorCollectionClearCodec.EncodeRequest(Name);
            using var _ = await Cluster.Messaging.SendAsync(message).CfAwait();
        }
        public async Task<long> GetSizeAsync()
        {
            using var message = VectorCollectionSizeCodec.EncodeRequest(Name);
            using var response = await Cluster.Messaging.SendAsync(message).CfAwait();
            return VectorCollectionSizeCodec.DecodeResponse(response).Response;
        }
        public async Task<IVectorSearchResult<TKey, TVal>> SearchAsync(VectorValues vectorValues, VectorSearchOptions searchOptions)
        {
            searchOptions.ThrowIfNull();
            using var message = VectorCollectionSearchNearVectorCodec.EncodeRequest(Name, vectorValues, searchOptions);
            using var response = await Cluster.Messaging.SendAsync(message).CfAwait();
            var rawResponse = VectorCollectionSearchNearVectorCodec.DecodeResponse(response).Result;

            var entries = new List<VectorSearchResultEntry<TKey, TVal>>();
            foreach (var rawEntry in rawResponse)
            {
                var key = await ToObjectAsync<TKey>(rawEntry.Key).CfAwait();
                var value = await ToObjectAsync<TVal>(rawEntry.Value).CfAwait();
                var entry = new VectorSearchResultEntry<TKey, TVal>(key, value, rawEntry.Vectors, rawEntry.Score);
                entries.Add(entry);
            }

            return new VectorSearchResult<TKey, TVal>(entries.Count, entries);
        }

        private async Task<IVectorDocument<TVal>> DeserializeIVectorDocumentAsync<TVal>(IVectorDocument<IData> rawResponse)
        {
            if (rawResponse is null)
            {
                return null;
            }

            var userObject = await ToObjectAsync<TVal>(rawResponse.Value).CfAwait();
            return new VectorDocument<TVal>(userObject, rawResponse.Vectors);
        }
        private (IData dataKey, IVectorDocument<IData> rawDocument) PrepareForPut(TKey key, IVectorDocument<TVal> valueIVectorDocument)
        {
            var dataKey = ToSafeData(key);
            var dataValue = ToSafeData(valueIVectorDocument.Value);
            var rawDocument = new VectorDocument<IData>(dataValue, valueIVectorDocument.Vectors);
            return (dataKey, rawDocument);
        }
    }
}
