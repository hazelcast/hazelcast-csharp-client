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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Implementation.List
{
    internal partial class HList<T> // Setting
    {
        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<T> SetAsync(int index, T item, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(DefaultOperationTimeoutMilliseconds);
            var task = SetAsync(index, item, cancellation.Token).OrTimeout(cancellation);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<T> SetAsync(int index, T item, CancellationToken cancellationToken)
        {
            var itemData = ToSafeData(item);
            var requestMessage = ListSetCodec.EncodeRequest(Name, index, itemData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            var response = ListSetCodec.DecodeResponse(responseMessage).Response;
            return ToObject<T>(response);
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task InsertAsync(int index, T item, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(DefaultOperationTimeoutMilliseconds);
            var task = InsertAsync(index, item, cancellation.Token).OrTimeout(cancellation);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task InsertAsync(int index, T item, CancellationToken cancellationToken)
        {
            var itemData = ToSafeData(item);
            var requestMessage = ListAddWithIndexCodec.EncodeRequest(Name, index, itemData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            _ = ListSetCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public override async Task<bool> AddAsync(T item, CancellationToken cancellationToken)
        {
            var itemData = ToSafeData(item);
            var requestMessage = ListAddCodec.EncodeRequest(Name, itemData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return ListAddCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public override async Task<bool> AddRangeAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = ListAddAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return ListAddAllCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<bool> InsertRangeAsync<TItem>(int index, ICollection<TItem> items, TimeSpan timeout = default)
            where TItem : T
        {
            var cancellation = timeout.AsCancellationTokenSource(DefaultOperationTimeoutMilliseconds);
            var task = InsertRangeAsync(index, items, cancellation.Token).OrTimeout(cancellation);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<bool> InsertRangeAsync<TItem>(int index, ICollection<TItem> items, CancellationToken cancellationToken)
            where TItem : T
        {
            var itemsData = ToSafeData(items);
            var requestMessage = ListAddAllWithIndexCodec.EncodeRequest(Name, index, itemsData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return ListAddAllWithIndexCodec.DecodeResponse(responseMessage).Response;
        }

    }
}