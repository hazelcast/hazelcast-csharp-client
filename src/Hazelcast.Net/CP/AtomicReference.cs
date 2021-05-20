using System;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.CP
{
    /// <summary>
    /// Provides the <see cref="IAtomicReference{T}"/> implementation.
    /// </summary>
    internal class AtomicReference<T>: CPDistributedObjectBase, IAtomicReference<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicReference{T}"/> class.
        /// </summary>
        /// <param name="name">The unique name.</param>
        /// <param name="groupId">The CP group identifier.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="serializationService">The serialization service.</param>
        public AtomicReference(string name, CPGroupId groupId, Cluster cluster, SerializationService serializationService)
            : base(ServiceNames.AtomicRef, name, groupId, cluster)
        {
            SerializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
        }

        protected SerializationService SerializationService { get; }

        /// <inheritdoc />
        public async Task<bool> CompareAndSetAsync(T comparand, T value)
        {
            var requestMessage = AtomicRefCompareAndSetCodec.EncodeRequest(CPGroupId, Name, ToData(comparand), ToData(value));
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = AtomicRefCompareAndSetCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<T> GetAsync()
        {
            var requestMessage = AtomicRefGetCodec.EncodeRequest(CPGroupId, Name);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = AtomicRefGetCodec.DecodeResponse(responseMessage).Response;
            return ToObject(response);
        }

        /// <inheritdoc />
        public async Task SetAsync(T value)
        {
            var requestMessage = AtomicRefSetCodec.EncodeRequest(CPGroupId, Name, ToData(value), returnOldValue: false);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = AtomicRefSetCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<T> GetAndSetAsync(T value)
        {
            var requestMessage = AtomicRefSetCodec.EncodeRequest(CPGroupId, Name, ToData(value), returnOldValue: true);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = AtomicRefSetCodec.DecodeResponse(responseMessage).Response;
            return ToObject(response);
        }

        /// <inheritdoc />
        public async Task<bool> IsNullAsync()
        {
            var requestMessage = AtomicRefContainsCodec.EncodeRequest(CPGroupId, Name, null);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = AtomicRefContainsCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task ClearAsync()
        {
            var requestMessage = AtomicRefSetCodec.EncodeRequest(CPGroupId, Name, null, returnOldValue: false);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = AtomicRefSetCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<bool> ContainsAsync(T value)
        {
            var requestMessage = AtomicRefContainsCodec.EncodeRequest(CPGroupId, Name, ToData(value));
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = AtomicRefContainsCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public override async ValueTask DestroyAsync()
        {
            var requestMessage = CPGroupDestroyCPObjectCodec.EncodeRequest(CPGroupId, ServiceName, Name);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = CPGroupDestroyCPObjectCodec.DecodeResponse(responseMessage);
        }

        protected IData ToData(T value) => SerializationService.ToData(value);
        protected T ToObject(IData data) => SerializationService.ToObject<T>(data);
    }
}