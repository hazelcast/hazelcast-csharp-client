using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.HTxSetImpl
{
    internal class HTxSet<TItem> : TransactionalDistributedObjectBase, IHTxSet<TItem>
    {
        public HTxSet(string name, Cluster cluster, Client transactionClient, Guid transactionId, ISerializationService serializationService, ILoggerFactory loggerFactory) 
            : base(HSet.ServiceName, name, cluster, transactionClient, transactionId, serializationService, loggerFactory)
        { }

        public Task<bool> AddAsync(TItem item, TimeSpan timeout = default)
            => TaskEx.WithTimeout(AddAsync, item, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<bool> AddAsync(TItem item, CancellationToken cancellationToken)
        {
            var itemData = ToSafeData(item);
            var requestMessage = TransactionalSetAddCodec.EncodeRequest(Name, TransactionId, ThreadId, itemData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalSetAddCodec.DecodeResponse(responseMessage).Response;
        }

        public Task<bool> RemoveAsync(TItem item, TimeSpan timeout = default)
            => TaskEx.WithTimeout(RemoveAsync, item, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<bool> RemoveAsync(TItem item, CancellationToken cancellationToken)
        {
            var itemData = ToSafeData(item);
            var requestMessage = TransactionalSetRemoveCodec.EncodeRequest(Name, TransactionId, ThreadId, itemData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalSetRemoveCodec.DecodeResponse(responseMessage).Response;
        }

        public Task<int> CountAsync(TimeSpan timeout = default)
            => TaskEx.WithTimeout(CountAsync, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            var requestMessage = TransactionalSetSizeCodec.EncodeRequest(Name, TransactionId, ThreadId);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalSetSizeCodec.DecodeResponse(responseMessage).Response;
        }
    }
}
