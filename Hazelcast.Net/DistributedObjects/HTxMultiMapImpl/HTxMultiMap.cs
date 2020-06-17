using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Collections;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.HTxMultiMapImpl
{
    internal class HTxMultiMap<TKey, TValue> : TransactionalDistributedObjectBase, IHTxMultiMap<TKey, TValue>
    {
        public HTxMultiMap(string name, Cluster cluster, Client transactionClient, Guid transactionId, ISerializationService serializationService, ILoggerFactory loggerFactory) 
            : base(HMultiMap.ServiceName, name, cluster, transactionClient, transactionId, serializationService, loggerFactory)
        { }

        public Task<IReadOnlyList<TValue>> GetAsync(TKey key, TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetAsync, key, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<IReadOnlyList<TValue>> GetAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMultiMapGetCodec.EncodeRequest(Name, TransactionId, ThreadId, keyData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            var response = TransactionalMultiMapGetCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TValue>(response, SerializationService);
        }

        public Task<bool> TryAddAsync(TKey key, TValue value, TimeSpan timeout = default)
            => TaskEx.WithTimeout(TryAddAsync, key, value, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<bool> TryAddAsync(TKey key, TValue value, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var requestMessage = TransactionalMultiMapPutCodec.EncodeRequest(Name, TransactionId, ThreadId, keyData, valueData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalMultiMapPutCodec.DecodeResponse(responseMessage).Response;
        }

        public Task<bool> RemoveAsync(TKey key, TValue value, TimeSpan timeout = default)
            => TaskEx.WithTimeout(RemoveAsync, key, value, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<bool> RemoveAsync(TKey key, TValue value, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var requestMessage = TransactionalMultiMapRemoveEntryCodec.EncodeRequest(Name, TransactionId, ThreadId, keyData, valueData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalMultiMapRemoveEntryCodec.DecodeResponse(responseMessage).Response;
        }

        public Task<IReadOnlyList<TValue>> RemoveAsync(TKey key, TimeSpan timeout = default)
            => TaskEx.WithTimeout(RemoveAsync, key, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<IReadOnlyList<TValue>> RemoveAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToData(key);
            var requestMessage = TransactionalMultiMapRemoveCodec.EncodeRequest(Name, TransactionId, ThreadId, keyData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            var response = TransactionalMultiMapRemoveCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TValue>(response, SerializationService);
        }

        public Task<int> CountAsync(TimeSpan timeout = default)
            => TaskEx.WithTimeout(CountAsync, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            var requestMessage = TransactionalMultiMapSizeCodec.EncodeRequest(Name, TransactionId, ThreadId);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalMultiMapSizeCodec.DecodeResponse(responseMessage).Response;
        }

        public Task<int> ValueCountAsync(TKey key, TimeSpan timeout = default)
            => TaskEx.WithTimeout(ValueCountAsync, key, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<int> ValueCountAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMultiMapValueCountCodec.EncodeRequest(Name, TransactionId, ThreadId, keyData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalMultiMapValueCountCodec.DecodeResponse(responseMessage).Response;
        }
    }
}
