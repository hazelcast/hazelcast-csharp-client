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

using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.CP
{
    /// <summary>
    /// Provides the <see cref="IAtomicLong"/> implementation.
    /// </summary>
    internal class AtomicLong : CPDistributedObjectBase, IAtomicLong
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicLong"/> class.
        /// </summary>
        /// <param name="name">The unique name.</param>
        /// <param name="groupId">The CP group identifier.</param>
        /// <param name="cluster">The cluster.</param>
        public AtomicLong(string name, CPGroupId groupId, Cluster cluster)
            : base(ServiceNames.AtomicLong, name, groupId, cluster)
        { }

        /// <inheritdoc />
        public async Task<long> AddAndGetAsync(long value)
        {
            var requestMessage = AtomicLongAddAndGetCodec.EncodeRequest(CPGroupId, Name, value);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = AtomicLongAddAndGetCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<bool> CompareAndSetAsync(long comparand, long value)
        {
            var requestMessage = AtomicLongCompareAndSetCodec.EncodeRequest(CPGroupId, Name, comparand, value);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = AtomicLongCompareAndSetCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<long> GetAndAddAsync(long value)
        {
            var requestMessage = AtomicLongGetAndAddCodec.EncodeRequest(CPGroupId, Name, value);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = AtomicLongGetAndAddCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<long> GetAndSetAsync(long value)
        {
            var requestMessage = AtomicLongGetAndSetCodec.EncodeRequest(CPGroupId, Name, value);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = AtomicLongGetAndSetCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public Task<long> DecrementAndGetAsync() => AddAndGetAsync(-1);

        /// <inheritdoc />
        public Task<long> GetAndDecrementAsync() => GetAndAddAsync(-1);

        /// <inheritdoc />
        public async Task<long> GetAsync()
        {
            var requestMessage = AtomicLongGetCodec.EncodeRequest(CPGroupId, Name);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = AtomicLongGetCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public Task<long> IncrementAndGetAsync() => AddAndGetAsync(+1);

        /// <inheritdoc />
        public Task<long> GetAndIncrementAsync() => GetAndAddAsync(+1);

        /// <inheritdoc />
        public Task SetAsync(long value) => GetAndSetAsync(value);

        /// <inheritdoc />
        public override async ValueTask DestroyAsync()
        {
            var requestMessage = CPGroupDestroyCPObjectCodec.EncodeRequest(CPGroupId, ServiceName, Name);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = CPGroupDestroyCPObjectCodec.DecodeResponse(responseMessage);
        }
    }
}
