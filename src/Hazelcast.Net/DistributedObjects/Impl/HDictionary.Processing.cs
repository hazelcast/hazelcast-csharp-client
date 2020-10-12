// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Predicates;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Impl
{
    // ReSharper disable once UnusedTypeParameter
    internal partial class HDictionary<TKey, TValue> // Processing
    {
        /// <inheritdoc />
        public Task<TResult> ExecuteAsync<TResult>(IEntryProcessor<TResult> processor, TKey key)
            => ExecuteAsync<TResult>(processor, key, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<TResult> ExecuteAsync<TResult>(IEntryProcessor<TResult> processor, TKey key, CancellationToken cancellationToken)
        {
            var (keyData, processorData) = ToSafeData(key, processor);
            var task = ExecuteAsync<TResult>(processorData, keyData, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <summary>
        /// Processes an entry.
        /// </summary>
        /// <param name="keyData">The key.</param>
        /// <param name="processorData">An entry processor.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The result of the process.</returns>
        /// <remarks>
        /// <para>The <paramref name="processorData"/> must have a counterpart on the server.</para>
        /// </remarks>
        protected virtual async Task<TResult> ExecuteAsync<TResult>(IData processorData, IData keyData, CancellationToken cancellationToken)
        {
            var requestMessage = MapExecuteOnKeyCodec.EncodeRequest(Name, processorData, keyData, ContextId);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CAF();
            var response = MapExecuteOnKeyCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TResult>(response);
        }

        /// <inheritdoc />
        public Task<IDictionary<TKey, TResult>> ExecuteAsync<TResult>(IEntryProcessor<TResult> processor, IEnumerable<TKey> keys)
            => ExecuteAsync<TResult>(processor, keys, CancellationToken.None);

        private async Task<IDictionary<TKey, TResult>> ExecuteAsync<TResult>(IEntryProcessor<TResult> processor, IEnumerable<TKey> keys, CancellationToken cancellationToken)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            var keysmap = keys.ToDictionary(x => ToSafeData(x), x => x);
            if (keysmap.Count == 0) return new Dictionary<TKey, TResult>();
            var processorData = ToSafeData(processor);

            var requestMessage = MapExecuteOnKeysCodec.EncodeRequest(Name, processorData, keysmap.Keys);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CAF();
            var response = MapExecuteOnKeysCodec.DecodeResponse(responseMessage).Response;

            var result = new Dictionary<TKey, TResult>();
            foreach (var (keyData, valueData) in response)
            {
                if (!keysmap.TryGetValue(keyData, out var key))
                    throw new InvalidOperationException("Server returned an unexpected key.");
                result[key] = ToObject<TResult>(valueData);
            }

            return result;
        }

        /// <inheritdoc />
        public Task<IDictionary<TKey, TResult>> ExecuteAsync<TResult>(IEntryProcessor<TResult> processor)
            => ExecuteAsync<TResult>(processor, CancellationToken.None);

        private async Task<IDictionary<TKey, TResult>> ExecuteAsync<TResult>(IEntryProcessor<TResult> processor, CancellationToken cancellationToken)
        {
            var processorData = ToSafeData(processor);

            var requestMessage = MapExecuteOnAllKeysCodec.EncodeRequest(Name, processorData);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CAF();
            var response = MapExecuteOnAllKeysCodec.DecodeResponse(responseMessage).Response;

            var result = new Dictionary<TKey, TResult>();
            foreach (var (keyData, valueData) in response)
                result[ToObject<TKey>(keyData)] = ToObject<TResult>(valueData);
            return result;

        }

        /// <inheritdoc />
        public Task<IDictionary<TKey, TResult>> ExecuteAsync<TResult>(IEntryProcessor<TResult> processor, IPredicate predicate)
            => ExecuteAsync<TResult>(processor, predicate, CancellationToken.None);

        private async Task<IDictionary<TKey, TResult>> ExecuteAsync<TResult>(IEntryProcessor<TResult> processor, IPredicate predicate, CancellationToken cancellationToken)
        {
            var (processorData, predicateData) = ToSafeData(processor, predicate);

            var requestMessage = MapExecuteWithPredicateCodec.EncodeRequest(Name, processorData, predicateData);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CAF();
            var response = MapExecuteWithPredicateCodec.DecodeResponse(responseMessage).Response;

            var result = new Dictionary<TKey, TResult>();
            foreach (var (keyData, valueData) in response)
                result[ToObject<TKey>(keyData)] = ToObject<TResult>(valueData);
            return result;

        }
    }
}
