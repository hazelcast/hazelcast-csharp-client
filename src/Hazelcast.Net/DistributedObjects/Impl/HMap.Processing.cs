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
    internal partial class HMap<TKey, TValue> // Processing
    {
        /// <inheritdoc />
        public Task<object> ExecuteAsync(IEntryProcessor processor, TKey key)
            => ExecuteAsync(processor, key, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<object> ExecuteAsync(IEntryProcessor processor, TKey key, CancellationToken cancellationToken)
        {
            var (keyData, processorData) = ToSafeData(key, processor);
            var task = ExecuteAsync(processorData, keyData, cancellationToken);

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
        protected virtual async Task<object> ExecuteAsync(IData processorData, IData keyData, CancellationToken cancellationToken)
        {
            var requestMessage = MapExecuteOnKeyCodec.EncodeRequest(Name, processorData, keyData, ContextId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CAF();
            var response = MapExecuteOnKeyCodec.DecodeResponse(responseMessage).Response;
            return ToObject<object>(response);
        }

        /// <inheritdoc />
        public Task<IDictionary<TKey, object>> ExecuteAsync(IEntryProcessor processor, IEnumerable<TKey> keys)
            => ExecuteAsync(processor, keys, CancellationToken.None);

        private async Task<IDictionary<TKey, object>> ExecuteAsync(IEntryProcessor processor, IEnumerable<TKey> keys, CancellationToken cancellationToken)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            var keysmap = keys.ToDictionary(x => ToSafeData(x), x => x);
            if (keysmap.Count == 0) return new Dictionary<TKey, object>();
            var processorData = ToSafeData(processor);

            var requestMessage = MapExecuteOnKeysCodec.EncodeRequest(Name, processorData, keysmap.Keys);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            var response = MapExecuteOnKeysCodec.DecodeResponse(responseMessage).Response;

            var result = new Dictionary<TKey, object>();
            foreach (var (keyData, valueData) in response)
            {
                if (!keysmap.TryGetValue(keyData, out var key))
                    throw new InvalidOperationException("Server returned an unexpected key.");
                result[key] = ToObject<object>(valueData);
            }

            return result;
        }

        /// <inheritdoc />
        public Task<IDictionary<TKey, object>> ExecuteAsync(IEntryProcessor processor)
            => ExecuteAsync(processor, CancellationToken.None);

        private async Task<IDictionary<TKey, object>> ExecuteAsync(IEntryProcessor processor, CancellationToken cancellationToken)
        {
            var processorData = ToSafeData(processor);

            var requestMessage = MapExecuteOnAllKeysCodec.EncodeRequest(Name, processorData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            var response = MapExecuteOnAllKeysCodec.DecodeResponse(responseMessage).Response;

            var result = new Dictionary<TKey, object>();
            foreach (var (keyData, valueData) in response)
                result[ToObject<TKey>(keyData)] = ToObject<object>(valueData);
            return result;

        }

        /// <inheritdoc />
        public Task<IDictionary<TKey, object>> ExecuteAsync(IEntryProcessor processor, IPredicate predicate)
            => ExecuteAsync(processor, predicate, CancellationToken.None);

        private async Task<IDictionary<TKey, object>> ExecuteAsync(IEntryProcessor processor, IPredicate predicate, CancellationToken cancellationToken)
        {
            var (processorData, predicateData) = ToSafeData(processor, predicate);

            var requestMessage = MapExecuteWithPredicateCodec.EncodeRequest(Name, processorData, predicateData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            var response = MapExecuteWithPredicateCodec.DecodeResponse(responseMessage).Response;

            var result = new Dictionary<TKey, object>();
            foreach (var (keyData, valueData) in response)
                result[ToObject<TKey>(keyData)] = ToObject<object>(valueData);
            return result;

        }

        /// <inheritdoc />
        public Task<object> ApplyAsync(IEntryProcessor processor, TKey key)
            => ApplyAsync(processor, key, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<object> ApplyAsync(IEntryProcessor processor, TKey key, CancellationToken cancellationToken)
        {
            var (keyData, processorData) = ToSafeData(key, processor);
            var task = ApplyAsync(processorData, keyData, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        // TODO: understand what this does, and document
        //
        // documentation says that... "execute processes ..., blocking until the processing is
        // complete and the result is returned" and "submit processes ... and provides a way to
        // register a callback to receive notifications about the result of the entry processing".
        //
        // in the original code, "execute" returns a value whereas "submit" returns a task that
        // can be awaited - but since we are fully async now, all our code now returns tasks.
        //
        // however, "execute" uses MapExecuteOnKeyCodec (77312 // 0x012E00) whereas "submit" uses
        // MapSubmitToKeyCodec (77568 // 0x012F00) which is a different codec

        protected virtual async Task<object> ApplyAsync(IData processorData, IData keyData, CancellationToken cancellationToken)
        {
            var requestMessage = MapSubmitToKeyCodec.EncodeRequest(Name, processorData, keyData, ContextId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CAF();
            var response = MapSubmitToKeyCodec.DecodeResponse(responseMessage).Response;
            return ToObject<object>(response);
        }
    }
}
