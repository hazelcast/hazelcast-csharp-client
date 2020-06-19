using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Clustering;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.HTxListImpl
{
    /// <summary>
    /// Implements <see cref="IHTxList{TItem}"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    internal class HTxList<TItem> : TransactionalDistributedObjectBase, IHTxList<TItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HTxList{TItem}"/> class.
        /// </summary>
        /// <param name="name">The unique name of the list.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="transactionClient">The client supporting the transaction.</param>
        /// <param name="transactionId">The unique identifier of the transaction.</param>
        /// <param name="serializationService">The serialization service.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public HTxList(string name, Cluster cluster, Client transactionClient, Guid transactionId, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : base(HList.ServiceName, name, cluster, transactionClient, transactionId, serializationService, loggerFactory)
        { }

        /// <inheritoc />
        public async Task<bool> AddAsync(TItem item, CancellationToken cancellationToken)
        {
            var itemData = ToData(item);
            var requestMessage = TransactionalListAddCodec.EncodeRequest(Name, TransactionId, ThreadId, itemData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalListAddCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritoc />
        public async Task<bool> RemoveAsync(TItem item, CancellationToken cancellationToken)
        {
            var itemData = ToData(item);
            var requestMessage = TransactionalListRemoveCodec.EncodeRequest(Name, TransactionId, ThreadId, itemData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalListRemoveCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritoc />
        public async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            var requestMessage = TransactionalListSizeCodec.EncodeRequest(Name, TransactionId, ThreadId);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalListSizeCodec.DecodeResponse(responseMessage).Response;
        }
    }
}
